#if DEBUG
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DebugController> _logger;

        public DebugController(ITaskDataConnection db, ILogger<DebugController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// ЧИТ-КОД 6: Массовый чекин.
        /// Всем сотрудникам в базе ставится отметка "in" (вход) на указанный филиал.
        /// Время отметки: текущее UTC минус 30 минут.
        /// </summary>
        [HttpPost("employees/force-checkin-all")]
        public async Task<IActionResult> ForceCheckInAllEmployees([FromQuery] int branchId = 1)
        {
            _logger.LogWarning("|   [DEBUG] Запущен массовый чекин всех сотрудников на филиал {BranchId}", branchId);
            var db = (DataConnection)_db;

            // Вычисляем время: текущее минус 30 минут
            var checkTime = DateTime.UtcNow.AddMinutes(-30);

            using var transaction = await db.BeginTransactionAsync();
            try
            {
                // 1. Получаем ID всех существующих сотрудников
                var employeeIds = await db.GetTable<EmployeeModel>()
                    .Select(e => e.EmployeesId)
                    .ToListAsync();

                if (!employeeIds.Any())
                {
                    _logger.LogWarning("|   [DEBUG] Сотрудники не найдены в таблице employees");
                    return BadRequest("В базе данных нет сотрудников для выполнения чекина.");
                }

                // 2. Создаем отметки "in" для каждого сотрудника
                foreach (var empId in employeeIds)
                {
                    await db.InsertAsync(new CheckIOEmployeeModel
                    {
                        EmployeeId = empId,
                        BranchId = branchId,
                        CheckType = "in",
                        CheckTimeStamp = checkTime
                    });
                }

                await transaction.CommitAsync();

                _logger.LogInformation("|   [DEBUG] Массовый чекин завершен. Зачекинено: {Count} чел. Время: {Time}",
                    employeeIds.Count, checkTime);

                return Ok(new
                {
                    Message = $"[Магия] {employeeIds.Count} сотрудников успешно отмечены как 'в офисе'.",
                    Timestamp = checkTime
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "|   !!! [DEBUG] Ошибка при выполнении ForceCheckInAllEmployees");
                return StatusCode(500, $"Ошибка при массовом чекине: {ex.Message}");
            }
        }

        /// <summary>
        /// ЧИТ-КОД 1: Пропускает сборку. Переводит заказ в статус Ready.
        /// ИСПРАВЛЕНО: Теперь принудительно генерирует задачу на выдачу со строками позиций для тестирования в мобилке.
        /// </summary>
        [HttpPost("orders/{orderId}/skip-assembly")]
        public async Task<IActionResult> SkipAssembly(int orderId)
        {
            _logger.LogInformation("|   [DEBUG] Запущен чит-код SkipAssembly для заказа #{OrderId}", orderId);
            var db = (DataConnection)_db;
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                var order = await db.GetTable<OrderModel>().FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    _logger.LogWarning("|   [DEBUG] Заказ #{OrderId} не найден", orderId);
                    return NotFound($"Заказ {orderId} не найден.");
                }

                // 1. Меняем статус заказа на Ready
                await db.GetTable<OrderModel>()
                    .Where(o => o.OrderId == orderId)
                    .Set(o => o.Status, "Ready")
                    .UpdateAsync();

                // 2. Убиваем зависшие задачи сборки
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

                // 3. ГЕНЕРАЦИЯ ЗАДАЧИ НА ВЫДАЧУ (Для мобильного приложения)
                bool hasHandover = await db.GetTable<OrderHandoverAssignmentModel>().AnyAsync(a => a.OrderId == orderId);

                if (!hasHandover)
                {
                    // А) Создаем базовую задачу в пуле
                    int taskId = await db.InsertWithInt32IdentityAsync(new BaseTaskModel
                    {
                        Title = $"Выдача заказа #{orderId}",
                        Type = "OrderHandover",
                        BranchId = order.BranchId,
                        PriorityLevel = 1,
                        Status = "New", // Задача доступна для взятия в работу
                        CreatedAt = DateTime.UtcNow
                    });

                    // Б) Создаем назначение для сотрудника (пока без привязки к конкретному WorkerId)
                    int assignmentId = await db.InsertWithInt32IdentityAsync(new OrderHandoverAssignmentModel
                    {
                        TaskId = taskId,
                        OrderId = orderId,
                        HandoverType = (order.DeliveryType == "Express" || order.DeliveryType == "Delivery") ? "ToCourier" : "ToCustomer",
                        Status = 0, // 0 = New/Unassigned
                        Role = "Main",
                        Complexity = 1,
                        AssignedAt = DateTime.UtcNow
                    });

                    // В) Важно: Создаем строки выдачи (Lines), иначе в режиме отмены будет пусто
                    var orderPositions = await db.GetTable<OrderPositionModel>().Where(op => op.OrderId == orderId).ToListAsync();

                    foreach (var pos in orderPositions)
                    {
                        var reservation = await db.GetTable<OrderReservationModel>().FirstOrDefaultAsync(r => r.OrderPositionId == pos.UniqueId);

                        await db.InsertAsync(new OrderHandoverLineModel
                        {
                            OrderHandoverAssignmentId = assignmentId,
                            OrderPositionId = pos.UniqueId,
                            ItemPositionId = reservation?.ItemPositionId,
                            Quantity = pos.Quantity,
                            ScannedQuantity = 0,
                            CancelledQuantity = 0
                        });
                    }

                    _logger.LogInformation("|   [DEBUG] Сгенерирована задача выдачи TaskId: {TaskId} для заказа #{OrderId}", taskId, orderId);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("|   [DEBUG] Магия сработала: Заказ #{OrderId} переведен в Ready и готов к выдаче", orderId);
                return Ok(new { Message = $"[Магия] Заказ #{orderId} собран по воздуху, сгенерирована задача на выдачу для мобилки." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "|   !!! [DEBUG] Ошибка в SkipAssembly");
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
            _logger.LogInformation("|   [DEBUG] Запущен чит-код SkipHandoverToCourier для заказа #{OrderId} на курьера #{CourierId}", orderId, courierId);
            var db = (DataConnection)_db;
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                var courierPosition = await db.GetTable<PositionModel>()
                    .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == courierId.ToString());

                if (courierPosition == null)
                {
                    _logger.LogWarning("|   [DEBUG] Виртуальная ячейка курьера #{CourierId} не найдена", courierId);
                    return BadRequest($"Виртуальная ячейка курьера {courierId} не найдена.");
                }

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
                _logger.LogInformation("|   [DEBUG] Заказ #{OrderId} телепортирован курьеру #{CourierId}", orderId, courierId);
                return Ok(new { Message = $"[Магия] Заказ #{orderId} телепортирован в багажник курьеру #{courierId} (InTransit)." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "|   !!! [DEBUG] Ошибка в SkipHandoverToCourier");
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
            _logger.LogInformation("|   [DEBUG] Запущена мягкая очистка застрявших задач (ClearStuckTasks)");
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
                _logger.LogInformation("|   [DEBUG] Очищено задач: {CancelledCount}. Сборки сброшены", cancelledCount);
                return Ok(new { Message = $"[Магия] Очищено зависших базовых задач: {cancelledCount}. База сброшена." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "|   !!! [DEBUG] Ошибка в ClearStuckTasks");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// ЧИТ-КОД 4: ГЛОБАЛЬНАЯ ОЧИСТКА БАЗЫ (Полный сброс)
        /// Аналог TRUNCATE ... RESTART IDENTITY CASCADE.
        /// Удаляет заказы, задачи, инвентаризацию и сбрасывает ID.
        /// </summary>
        [HttpPost("system/nuclear-reset")]
        public async Task<IActionResult> NuclearReset()
        {
            _logger.LogCritical("|   [DEBUG] ИНИЦИИРОВАН ЯДЕРНЫЙ СБРОС БАЗЫ ДАННЫХ (NuclearReset)!");
            var db = (DataConnection)_db;
            var sql = @"
                TRUNCATE TABLE 
                    inventory_discrepancies, 
                    inventory_statistics, 
                    inventory_assignment_lines, 
                    inventory_assignments, 
                    active_assigned_tasks, 
                    base_tasks, 
                    orders 
                RESTART IDENTITY CASCADE;";

            using var transaction = await db.BeginTransactionAsync();
            try
            {
                await db.ExecuteAsync(sql);
                await transaction.CommitAsync();
                _logger.LogWarning("|   [DEBUG] NUCLEAR RESET: Таблицы успешно очищены, каскадное удаление завершено, счетчики ID сброшены.");

                return Ok(new
                {
                    Message = "[Магия] База данных полностью очищена. Все ID сброшены. Каскадное удаление завершено."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "|   !!! [DEBUG] Ошибка при выполнении NuclearReset");
                return StatusCode(500, $"Ошибка при очистке: {ex.Message}");
            }
        }

        /// <summary>
        /// ЧИТ-КОД 5: Генератор профильных тестовых заказов.
        /// {type} может быть: "light" (обычный), "heavy" (с напарником), "express" (срочный)
        /// Добавлена возможность явно указать deliveryType (например, "Pickup" для самовывоза).
        /// </summary>
        [HttpPost("orders/generate-test/{type}")]
        public async Task<IActionResult> GenerateTestOrder(string type, [FromQuery] int customerId = 1, [FromQuery] int branchId = 1, [FromQuery] string? deliveryType = null)
        {
            _logger.LogInformation("|   [DEBUG] Генерация тестового заказа типа {Type} для клиента {CustomerId} на филиале {BranchId}. Доставка: {DeliveryType}", type, customerId, branchId, deliveryType ?? "По умолчанию");
            var db = (DataConnection)_db;
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                type = type.ToLower();

                // ДОБАВЛЕНО: Определяем тип доставки. Если передан явно — используем его, иначе дефолтная логика.
                string finalDeliveryType = deliveryType ?? (type == "express" ? "Express" : "Delivery");

                // 1. Создаем болванку заказа
                var order = new OrderModel
                {
                    CustomerId = customerId,
                    BranchId = branchId,
                    Status = "Created", // Создаем как "Новый", чтобы он прошел через распределитель склада
                    DeliveryType = finalDeliveryType, // ИСПОЛЬЗУЕМ НОВЫЙ ТИП ДОСТАВКИ
                    PaymentType = "Prepaid",
                    DestinationAddress = $"ул. Тестовая, д. {new Random().Next(1, 100)}",
                    TotalPrice = type == "heavy" ? 55000 : 1200,
                    CreatedAt = DateTime.UtcNow,
                    DeliveryDate = DateTime.UtcNow.AddHours(4) // Доставка через 4 часа
                };

                int newOrderId = await db.InsertWithInt32IdentityAsync(order);

                // 2. Подбираем подходящий товар из БД
                ItemModel? item = null;

                if (type == "heavy")
                {
                    // Ищем самый тяжелый товар на складе
                    item = await db.GetTable<ItemModel>()
                        .OrderByDescending(i => i.Weight)
                        .FirstOrDefaultAsync();
                }
                else if (type == "light")
                {
                    // Ищем самый легкий товар
                    item = await db.GetTable<ItemModel>()
                        .OrderBy(i => i.Weight)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    // Для express берем первый попавшийся обычный товар
                    item = await db.GetTable<ItemModel>().FirstOrDefaultAsync();
                }

                if (item == null)
                {
                    _logger.LogWarning("|   [DEBUG] Ошибка генерации: в БД нет товаров (ItemModel)");
                    return BadRequest("В базе данных нет ни одного товара (ItemModel) для создания заказа.");
                }

                // 3. Создаем позицию в заказе
                int quantity = type == "heavy" ? 5 : 1;

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
                    OrderPositionId = uniqueId,
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

                _logger.LogInformation("|   [DEBUG] {Message}", msg);

                return Ok(new
                {
                    OrderId = newOrderId,
                    ItemName = item.Name,
                    Weight = item.Weight,
                    Quantity = quantity,
                    DeliveryType = finalDeliveryType, // Выводим тип доставки для наглядности в Swagger
                    Message = msg
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "|   !!! [DEBUG] Ошибка в генераторе тестовых заказов");
                return StatusCode(500, $"Ошибка генератора: {ex.Message}");
            }
        }

        /// <summary>
        /// ЧИТ-КОД 4: Принудительный чекин курьера (Dock).
        /// Запускает генерацию задач ReturnToStock для всех "отказников" в машине курьера.
        /// </summary>
        [HttpPost("courier/{courierId}/force-dock")]
        public async Task<IActionResult> ForceCourierDock(int courierId, [FromQuery] int branchId)
        {
            // Находим всех слушателей чекина (как это делает QrCheckInController)
            var observers = HttpContext.RequestServices.GetServices<TaskControl.Core.Shared.SharedInterfaces.IEmployeeCheckInObserver>();

            foreach (var observer in observers)
            {
                await observer.OnEmployeeCheckedAsync(courierId, branchId, "dock");
            }

            return Ok(new { Message = $"Событие 'Прибытие' для курьера {courierId} отправлено. Задачи возврата должны быть созданы." });
        }

        /// <summary>
        /// ДИАГНОСТИКА: Проверка состояния ячейки и её содержимого.
        /// </summary>
        [HttpGet("cell/{positionId}/inspect")]
        public async Task<IActionResult> InspectCell(int positionId)
        {
            var pos = await _db.GetTable<PositionModel>().FirstOrDefaultAsync(p => p.PositionId == positionId);
            var items = await _db.GetTable<ItemPositionModel>().Where(ip => ip.PositionId == positionId).ToListAsync();

            return Ok(new
            {
                CellStatus = pos?.Status,
                CellCode = pos?.FLSNumber,
                ItemsInCell = items.Select(i => new { i.ItemId, i.Quantity })
            });
        }
    }
}
#endif