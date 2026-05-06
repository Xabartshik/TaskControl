using LinqToDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.Application.DTOs;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CourierController : ControllerBase
    {
        private readonly IQRTokenService _qrTokenService;
        private readonly ITaskDataConnection _db;

        public CourierController(IQRTokenService qrTokenService, ITaskDataConnection db)
        {
            _qrTokenService = qrTokenService;
            _db = db;
        }

        /// <summary>
        /// Получить временный QR-код курьера для подтверждения приемки товаров от кладовщика.
        /// </summary>
        [HttpGet("{courierId}/pickup-qr")]
        public IActionResult GetPickupQrToken(int courierId)
        {
            if (courierId <= 0) return BadRequest(new { Message = "Некорректный ID курьера." });

            try
            {
                string token = _qrTokenService.GenerateCourierPickupToken(courierId);
                return Ok(new { Token = token, ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5), ExpiresInSeconds = 300 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при генерации QR-кода курьера.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получить все актуальные заказы курьера (ожидающие погрузки и те, что уже в пути)
        /// </summary>
        [HttpGet("{courierId}/orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetCourierOrders(int courierId)
        {
            try
            {
                // 1. Ищем ID всех заказов, которые назначены на этого курьера
                var assignedOrderIds = await _db.GetTable<OrderHandoverAssignmentModel>()
                    .Where(a => a.TargetCourierId == courierId)
                    .Select(a => a.OrderId)
                    .Distinct()
                    .ToListAsync();

                if (!assignedOrderIds.Any())
                    return Ok(new List<OrderDto>());

                // 2. Достаем сами заказы, которые еще не завершены и не отменены
                var orders = await _db.GetTable<OrderModel>()
                    .Where(o => assignedOrderIds.Contains(o.OrderId)
                                && (o.Status == "Ready" || o.Status == "InTransit"))
                    .ToListAsync();

                // 3. Маппим в DTO для отправки на мобилку (без лишних деталей позиций для экономии трафика)
                var dtos = orders.Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    CustomerId = o.CustomerId,
                    DestinationAddress = o.DestinationAddress,
                    Status = Enum.Parse<OrderModule.Domain.OrderStatus>(o.Status),
                    TotalPrice = o.TotalPrice,
                    DeliveryType = Enum.Parse<OrderModule.Domain.DeliveryType>(o.DeliveryType),
                    PaymentType = Enum.Parse<OrderModule.Domain.PaymentType>(o.PaymentType),
                    DeliveryDate = o.DeliveryDate
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении маршрутного листа.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Отказ клиента от заказа. Товар остается в багажнике курьера для возврата.
        /// </summary>
        [HttpPost("orders/{orderId}/reject")]
        public async Task<IActionResult> RejectDelivery(int orderId)
        {
            using var transaction = await ((LinqToDB.Data.DataConnection)_db).BeginTransactionAsync();
            try
            {
                // 1. Проверяем и переводим заказ в статус "Отказ"
                var updated = await _db.GetTable<OrderModel>()
                    .Where(o => o.OrderId == orderId && o.Status == "InTransit")
                    .Set(o => o.Status, "Rejected") // Используем статус, который добавили ранее
                    .UpdateAsync();

                if (updated == 0)
                {
                    return BadRequest(new { Message = "Заказ не найден или не находится в пути." });
                }

                // 2. Получаем все позиции заказа
                var orderPositions = await _db.GetTable<OrderPositionModel>()
                    .Where(op => op.OrderId == orderId)
                    .Select(op => op.UniqueId)
                    .ToListAsync();

                // 3. Снимаем резервы
                // Мы удаляем резервы, так как заказ отменен. 
                // Но сами записи ItemPositionModel в багажнике курьера МЫ НЕ ТРОГАЕМ.
                // Товар превращается в свободный "сироту" внутри машины курьера.
                await _db.GetTable<OrderReservationModel>()
                    .Where(r => orderPositions.Contains(r.OrderPositionId))
                    .DeleteAsync();

                await transaction.CommitAsync();
                return Ok(new { Message = "Отказ зафиксирован. Товар оставлен в багажнике для возврата на склад." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { Message = "Ошибка при оформлении отказа.", Details = ex.Message, Inner = ex.InnerException?.Message });
            }
        }

        /// <summary>
        /// Подтверждение доставки заказа клиенту со списанием товаров из виртуального багажника
        /// </summary>
        [HttpPost("orders/{orderId}/deliver")]
        public async Task<IActionResult> ConfirmDelivery(int orderId)
        {
            // Используем транзакцию, так как меняем баланс и статусы
            using var transaction = await ((LinqToDB.Data.DataConnection)_db).BeginTransactionAsync();
            try
            {
                // 1. Проверяем и переводим заказ в финальный статус
                var updated = await _db.GetTable<OrderModel>()
                    .Where(o => o.OrderId == orderId && o.Status == "InTransit")
                    .Set(o => o.Status, "Completed")
                    .UpdateAsync();

                if (updated == 0)
                {
                    return BadRequest(new { Message = "Заказ не найден или еще не был передан курьеру (статус не InTransit)." });
                }

                // 2. Получаем все позиции заказа
                var orderPositions = await _db.GetTable<OrderPositionModel>()
                    .Where(op => op.OrderId == orderId)
                    .ToListAsync();

                var reservationsTable = _db.GetTable<OrderReservationModel>();
                var itemPositionsTable = _db.GetTable<ItemPositionModel>();
                var movementsTable = _db.GetTable<ItemMovementModel>();

                // 3. Списываем каждый товар
                foreach (var op in orderPositions)
                {
                    // Ищем резерв, который привязан к этой позиции заказа
                    var reservation = await reservationsTable
                        .FirstOrDefaultAsync(r => r.OrderPositionId == op.UniqueId);

                    if (reservation != null && reservation.ItemPositionId.HasValue)
                    {
                        // Находим товар в багажнике курьера
                        var itemPos = await itemPositionsTable
                            .FirstOrDefaultAsync(ip => ip.Id == reservation.ItemPositionId.Value);

                        if (itemPos != null)
                        {
                            var remain = itemPos.Quantity - op.Quantity;

                            if (remain <= 0)
                            {
                                // Товар закончился - удаляем пустую ячейку
                                await itemPositionsTable.Where(ip => ip.Id == itemPos.Id).DeleteAsync();
                            }
                            else
                            {
                                // Вычитаем количество
                                await itemPositionsTable.Where(ip => ip.Id == itemPos.Id)
                                    .Set(ip => ip.Quantity, remain)
                                    .UpdateAsync();
                            }

                            // Логируем успешную доставку клиенту (DestinationPositionId = null означает уход со склада)
                            await _db.InsertAsync(new ItemMovementModel
                            {
                                ItemId = op.ItemId,
                                SourcePositionId = itemPos.PositionId, // Ячейка багажника
                                DestinationPositionId = null,          // Клиент
                                Quantity = op.Quantity,
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        // Удаляем выполненный резерв
                        await reservationsTable.Where(r => r.Id == reservation.Id).DeleteAsync();
                    }
                }

                await transaction.CommitAsync();
                return Ok(new { Message = "Заказ успешно доставлен и списан с баланса курьера!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "Ошибка при подтверждении доставки.", Details = ex.Message });
            }
        }


    }
}