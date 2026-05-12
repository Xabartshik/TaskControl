using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;

namespace TaskControl.DeliveryModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CourierDeliveryController : ControllerBase
    {
        private readonly ITaskDataConnection _db;
        private readonly IQRTokenService _qrTokenService;

        public CourierDeliveryController(ITaskDataConnection db, IQRTokenService qrTokenService)
        {
            _db = db;
            _qrTokenService = qrTokenService;
        }

        // Модель для частичной доставки
        public record PartialDeliveryRequest(string QrToken, int CourierId, Dictionary<int, int> RejectedQuantities);

        /// <summary>
        /// Частичный выкуп заказа курьером.
        /// </summary>
        [HttpPost("partial-complete")]
        public async Task<IActionResult> PartialCompleteDelivery([FromBody] PartialDeliveryRequest request)
        {
            // 1. Валидация QR клиента
            if (!_qrTokenService.ValidateOrderPickupToken(request.QrToken, out int customerId, out int orderId, out string error))
                return BadRequest($"Ошибка QR: {error}");

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                // Находим виртуальную ячейку курьера
                var courierPosition = await _db.GetTable<PositionModel>()
                    .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == request.CourierId.ToString());

                if (courierPosition == null)
                    return BadRequest("Виртуальная ячейка курьера не найдена.");

                var orderPositions = await _db.GetTable<OrderPositionModel>()
                    .Where(p => p.OrderId == orderId)
                    .ToListAsync();

                decimal newTotalPrice = 0;

                foreach (var op in orderPositions)
                {
                    // Получаем количество отмен (ключ - UniqueId позиции)
                    int rejectedQty = request.RejectedQuantities?.GetValueOrDefault(op.UniqueId) ?? 0;
                    int acceptedQty = op.Quantity - rejectedQty;

                    newTotalPrice += acceptedQty * op.Price;

                    // Обновляем количество в позиции заказа (может стать 0)
                    await _db.GetTable<OrderPositionModel>()
                        .Where(p => p.UniqueId == op.UniqueId)
                        .Set(p => p.Quantity, acceptedQty)
                        .UpdateAsync();

                    // Работа с физическими остатками и резервами
                    var reservation = await _db.GetTable<OrderReservationModel>()
                        .FirstOrDefaultAsync(r => r.OrderPositionId == op.UniqueId);

                    if (reservation != null && reservation.ItemPositionId.HasValue)
                    {
                        if (acceptedQty > 0)
                        {
                            // А. Пишем историю: из машины -> клиенту (DestinationPositionId = null)
                            await _db.InsertAsync(new ItemMovementModel
                            {
                                ItemId = op.ItemId,
                                SourcePositionId = courierPosition.PositionId,
                                DestinationPositionId = null,
                                Quantity = acceptedQty,
                                WorkerId = request.CourierId,
                                CreatedAt = DateTime.UtcNow
                            });

                            // Б. Уменьшаем физическое количество в "багажнике" курьера
                            await _db.GetTable<ItemPositionModel>()
                                .Where(ip => ip.Id == reservation.ItemPositionId.Value)
                                .Set(ip => ip.Quantity, ip => ip.Quantity - acceptedQty)
                                .UpdateAsync();
                        }

                        // ВАЖНО: Удаляем резерв. 
                        // Оставшиеся (отмененные) товары теперь лежат в ячейке курьера без привязки к заказу.
                        // Именно их потом найдет CourierReturnService при чекине "dock".
                        await _db.GetTable<OrderReservationModel>()
                            .Where(r => r.Id == reservation.Id)
                            .DeleteAsync();
                    }
                }

                // 4. Закрываем заказ с правильным статусом
                string finalStatus = newTotalPrice > 0 ? "Completed" : "Cancelled";

                await _db.GetTable<OrderModel>()
                    .Where(o => o.OrderId == orderId)
                    .Set(o => o.TotalPrice, newTotalPrice)
                    .Set(o => o.Status, finalStatus)
                    .UpdateAsync();

                await transaction.CommitAsync();

                return Ok(new { Message = $"Заказ #{orderId} успешно обработан. Статус: {finalStatus}, Сумма: {newTotalPrice}" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Ошибка при завершении доставки: " + ex.Message);
            }
        }

        public record CompleteDeliveryRequest(string QrToken, int CourierId);

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteDelivery([FromBody] CompleteDeliveryRequest request)
        {
            // 1. Валидация QR клиента
            if (!_qrTokenService.ValidateOrderPickupToken(request.QrToken, out int customerId, out int orderId, out string error))
                return BadRequest($"Ошибка QR-кода: {error}");

            // 2. Ищем виртуальную ячейку курьера (его багажник)
            var courierPosition = await _db.GetTable<PositionModel>()
                .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == request.CourierId.ToString());

            if (courierPosition == null)
                return BadRequest("Виртуальная ячейка курьера не найдена!");

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                // 3. Достаем все позиции заказа
                var orderPositions = await _db.GetTable<OrderPositionModel>()
                    .Where(op => op.OrderId == orderId)
                    .ToListAsync();

                var itemPositions = _db.GetTable<ItemPositionModel>();
                var reservations = _db.GetTable<OrderReservationModel>();

                foreach (var op in orderPositions)
                {
                    // Ищем этот товар в багажнике курьера
                    var itemPos = await itemPositions.FirstOrDefaultAsync(ip =>
                        ip.PositionId == courierPosition.PositionId &&
                        ip.ItemId == op.ItemId &&
                        ip.Quantity >= op.Quantity);

                    if (itemPos != null)
                    {
                        // А. Удаляем резерв (товар отдан клиенту)
                        await reservations.Where(r => r.OrderPositionId == op.UniqueId).DeleteAsync();

                        // Б. Списываем товар из машины курьера
                        var remaining = itemPos.Quantity - op.Quantity;
                        if (remaining <= 0)
                        {
                            // Очистка призрачных резервов (WMS Self-Healing)
                            await reservations.Where(r => r.ItemPositionId == itemPos.Id)
                                .Set(r => r.ItemPositionId, (int?)null).UpdateAsync();

                            await itemPositions.Where(ip => ip.Id == itemPos.Id).DeleteAsync();
                        }
                        else
                        {
                            await itemPositions.Where(ip => ip.Id == itemPos.Id)
                                .Set(ip => ip.Quantity, remaining).UpdateAsync();
                        }

                        // В. Пишем историю: из машины -> в руки клиенту (null)
                        await _db.InsertAsync(new ItemMovementModel
                        {
                            ItemId = op.ItemId,
                            SourcePositionId = remaining <= 0 ? (int?)null : courierPosition.PositionId,
                            DestinationPositionId = null,
                            Quantity = op.Quantity,
                            WorkerId = request.CourierId, // Курьер совершил операцию
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // 4. Закрываем заказ
                await _db.GetTable<OrderModel>()
                    .Where(o => o.OrderId == orderId)
                    .Set(o => o.Status, "Completed")
                    .UpdateAsync();

                await transaction.CommitAsync();

                return Ok(new { Message = $"Заказ #{orderId} успешно доставлен!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Ошибка при завершении доставки: " + ex.Message);
            }
        }





    }
}