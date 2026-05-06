#if DEBUG
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.Web.Controllers
{
    /// <summary>
    /// КОНТРОЛЛЕР ДЛЯ ТЕСТИРОВАНИЯ. 
    /// Доступен только в режиме сборки Debug. Позволяет "телепортировать" заказы между статусами.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly ITaskDataConnection _db;

        public DebugController(ITaskDataConnection db)
        {
            _db = db;
        }

        /// <summary>
        /// ЧИТ-КОД 1: Пропускает сборку. Переводит заказ в статус Ready.
        /// Идеально для тестирования экрана Логиста и Выдачи.
        /// </summary>
        [HttpPost("orders/{orderId}/skip-assembly")]
        public async Task<IActionResult> SkipAssembly(int orderId)
        {
            var db = (DataConnection)_db;
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                var updated = await db.GetTable<OrderModel>()
                    .Where(o => o.OrderId == orderId)
                    .Set(o => o.Status, "Ready")
                    .UpdateAsync();

                if (updated == 0) return NotFound($"Заказ {orderId} не найден.");

                // Убиваем зависшие задачи сборки
                var assemblyAssignments = await db.GetTable<OrderAssemblyAssignmentModel>()
                    .Where(a => a.OrderId == orderId)
                    .ToListAsync();

                foreach (var assignment in assemblyAssignments)
                {
                    await db.GetTable<BaseTaskModel>()
                        .Where(t => t.TaskId == assignment.TaskId)
                        .Set(t => t.Status, "Completed")
                        .UpdateAsync();

                    await db.GetTable<OrderAssemblyAssignmentModel>()
                        .Where(a => a.Id == assignment.Id)
                        .Set(a => a.Status, 2) // 2 = Completed
                        .UpdateAsync();
                }

                await transaction.CommitAsync();
                return Ok(new { Message = $"[Магия] Заказ #{orderId} собран по воздуху и готов к выдаче (Ready)." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// ЧИТ-КОД 2: Пропускает выдачу (кладовщика). Передает заказ сразу Курьеру.
        /// Идеально для тестирования мобильного приложения Курьера (доставка/отказ).
        /// </summary>
        [HttpPost("orders/{orderId}/give-to-courier/{courierId}")]
        public async Task<IActionResult> SkipHandoverToCourier(int orderId, int courierId)
        {
            var db = (DataConnection)_db;
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                var courierPosition = await db.GetTable<PositionModel>()
                    .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == courierId.ToString());

                if (courierPosition == null) return BadRequest($"Виртуальная ячейка курьера {courierId} не найдена.");

                // Меняем статус
                await db.GetTable<OrderModel>()
                    .Where(o => o.OrderId == orderId)
                    .Set(o => o.Status, "InTransit")
                    .UpdateAsync();

                // Телепортируем товары со склада в машину курьера
                var orderPositions = await db.GetTable<OrderPositionModel>().Where(op => op.OrderId == orderId).ToListAsync();
                foreach (var op in orderPositions)
                {
                    var reservation = await db.GetTable<OrderReservationModel>().FirstOrDefaultAsync(r => r.OrderPositionId == op.UniqueId);
                    if (reservation != null && reservation.ItemPositionId.HasValue)
                    {
                        var sourceItemPos = await db.GetTable<ItemPositionModel>().FirstOrDefaultAsync(ip => ip.Id == reservation.ItemPositionId.Value);
                        if (sourceItemPos != null)
                        {
                            // Создаем товар в багажнике
                            int newPosId = await db.InsertWithInt32IdentityAsync(new ItemPositionModel
                            {
                                ItemId = op.ItemId,
                                PositionId = courierPosition.PositionId,
                                Quantity = op.Quantity,
                                CreatedAt = DateTime.UtcNow
                            });

                            // Перепривязываем резерв
                            await db.GetTable<OrderReservationModel>().Where(r => r.Id == reservation.Id).Set(r => r.ItemPositionId, newPosId).UpdateAsync();

                            // Списываем со склада (упрощенно для дебага)
                            await db.GetTable<ItemPositionModel>().Where(ip => ip.Id == sourceItemPos.Id).Set(ip => ip.Quantity, ip => ip.Quantity - op.Quantity).UpdateAsync();
                        }
                    }
                }

                var handoverAssignments = await db.GetTable<OrderHandoverAssignmentModel>().Where(a => a.OrderId == orderId).ToListAsync();

                if (handoverAssignments.Any())
                {
                    // Если задача уже была создана логистом, просто завершаем её и прописываем ID курьера
                    foreach (var assignment in handoverAssignments)
                    {
                        await db.GetTable<BaseTaskModel>().Where(t => t.TaskId == assignment.TaskId).Set(t => t.Status, "Completed").UpdateAsync();
                        await db.GetTable<OrderHandoverAssignmentModel>()
                            .Where(a => a.Id == assignment.Id)
                            .Set(a => a.Status, 2)
                            .Set(a => a.TargetCourierId, courierId) // Привязываем к нашему курьеру
                            .UpdateAsync();
                    }
                }
                else
                {
                    // Получаем BranchId из заказа, чтобы задача не повисла в вакууме
                    var order = await db.GetTable<OrderModel>().FirstOrDefaultAsync(o => o.OrderId == orderId);
                    int branchId = order?.BranchId ?? 1;

                    // 1. Создаем фейковую базовую задачу и получаем ее реальный ID
                    int fakeTaskId = await db.InsertWithInt32IdentityAsync(new BaseTaskModel
                    {
                        Title = $"[Магия] Телепорт заказа #{orderId}",
                        Type = "OrderHandover", // Важно, чтобы тип совпадал
                        BranchId = branchId,
                        PriorityLevel = 1,
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow,
                        CompletedAt = DateTime.UtcNow
                    });

                    // 2. Создаем запись маршрута, привязанную к реальной задаче
                    await db.InsertAsync(new OrderHandoverAssignmentModel
                    {
                        TaskId = fakeTaskId, // <-- Теперь здесь легальный ID из БД!
                        OrderId = orderId,
                        TargetCourierId = courierId,
                        HandoverType = "ToCourier",
                        Status = 2, // Сразу "Выполнено"
                        Role = "Main",
                        Complexity = 1,
                        AssignedAt = DateTime.UtcNow,
                        CompletedAt = DateTime.UtcNow
                    });
                }

                await transaction.CommitAsync();
                return Ok(new { Message = $"[Магия] Заказ #{orderId} телепортирован в багажник курьеру #{courierId} (InTransit)." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// ЧИТ-КОД 3: Экстренная очистка (Сброс зависших задач)
        /// Если в процессе тестирования база засорилась мертвыми тасками.
        /// </summary>
        [HttpPost("tasks/clear-stuck")]
        public async Task<IActionResult> ClearStuckTasks()
        {
            var db = (DataConnection)_db;
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                // Отменяем все зависшие InProgress задачи, которые никто не трогал долгое время
                var cancelledCount = await db.GetTable<BaseTaskModel>()
                    .Where(t => t.Status == "InProgress")
                    .Set(t => t.Status, "Cancelled")
                    .UpdateAsync();

                // Возвращаем зависшие сборки в New
                await db.GetTable<OrderAssemblyAssignmentModel>()
                    .Where(a => a.Status == 1) // InProgress
                    .Set(a => a.Status, 0) // New
                    .Set(a => a.AssignedToUserId, (int?)null)
                    .UpdateAsync();

                await transaction.CommitAsync();
                return Ok(new { Message = $"[Магия] Очищено зависших базовых задач: {cancelledCount}. База сброшена." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// ЧИТ-КОД 4: Генератор профильных тестовых заказов.
        /// {type} может быть: "light" (обычный), "heavy" (с напарником), "express" (срочный)
        /// </summary>
        [HttpPost("orders/generate-test/{type}")]
        public async Task<IActionResult> GenerateTestOrder(string type, [FromQuery] int customerId = 1, [FromQuery] int branchId = 1)
        {
            var db = (DataConnection)_db;
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                type = type.ToLower();

                // 1. Создаем болванку заказа
                var order = new OrderModel
                {
                    CustomerId = customerId,
                    BranchId = branchId,
                    Status = "Created", // Создаем как "Новый", чтобы он прошел через распределитель склада
                    DeliveryType = type == "express" ? "Express" : "Delivery",
                    PaymentType = "Prepaid",
                    DestinationAddress = $"ул. Тестовая, д. {new Random().Next(1, 100)}",
                    TotalPrice = type == "heavy" ? 55000 : 1200,
                    CreatedAt = DateTime.UtcNow,
                    DeliveryDate = DateTime.UtcNow.AddHours(4) // Доставка через 4 часа
                };

                int newOrderId = await db.InsertWithInt32IdentityAsync(order);

                // 2. Подбираем подходящий товар из БД
                // ВАЖНО: Если у тебя в ItemModel поле веса называется иначе (Weight, WeightKg, WeightGrams), 
                // просто подставь его в сортировку OrderByDescending.
                ItemModel? item = null;

                if (type == "heavy")
                {
                    // Ищем самый тяжелый товар на складе (чтобы пробить лимит в 50кг для напарника)
                    // Предполагается, что в ItemModel есть поле Weight. 
                    // Если нет, просто закомментируй OrderByDescending и используй FirstAsync()
                    item = await db.GetTable<ItemModel>()
                        // .OrderByDescending(i => i.Weight) 
                        .FirstOrDefaultAsync();
                }
                else
                {
                    // Для light и express берем первый попавшийся обычный товар
                    item = await db.GetTable<ItemModel>().FirstOrDefaultAsync();
                }

                if (item == null) return BadRequest("В базе данных нет ни одного товара (ItemModel) для создания заказа.");

                // 3. Создаем позицию в заказе
                // 3. Создаем позицию в заказе
                int quantity = type == "heavy" ? 5 : 1;

                // Позволяем БД самой сгенерировать int UniqueId
                int uniqueId = await db.InsertWithInt32IdentityAsync(new OrderPositionModel
                {
                    OrderId = newOrderId,
                    ItemId = item.ItemId,
                    Quantity = quantity,
                    Price = 1000
                });

                // 4. Ищем любую складскую полку (Storage), чтобы положить туда товар
                var storagePosition = await db.GetTable<PositionModel>()
                    .FirstOrDefaultAsync(p => p.ZoneCode == "STORAGE" || p.ZoneCode == "A")
                    ?? await db.GetTable<PositionModel>().FirstAsync();

                // 5. Физически "кладем" товар на полку
                int stockId = await db.InsertWithInt32IdentityAsync(new ItemPositionModel
                {
                    ItemId = item.ItemId,
                    PositionId = storagePosition.PositionId,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow
                });

                // 6. Резервируем этот товар под наш заказ
                await db.InsertAsync(new OrderReservationModel
                {
                    OrderPositionId = uniqueId, // Теперь передается правильный сгенерированный int!
                    ItemPositionId = stockId,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow
                });

                await transaction.CommitAsync();

                string msg = type switch
                {
                    "heavy" => $"[Магия] Тяжелый заказ #{newOrderId} успешно сгенерирован! Он должен потребовать напарника.",
                    "express" => $"[Магия] Срочный Экспресс-заказ #{newOrderId} сгенерирован!",
                    _ => $"[Магия] Обычный заказ #{newOrderId} сгенерирован."
                };

                return Ok(new { OrderId = newOrderId, Message = msg });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Ошибка генератора: {ex.Message}");
            }
        }
    }


}
#endif