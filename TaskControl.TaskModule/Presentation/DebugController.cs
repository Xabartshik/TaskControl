#if DEBUG
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Application.Services; // <-- ДОБАВЛЕНО ДЛЯ СЕРВИСА

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
        private readonly ReturnTaskGeneratorService _returnTaskGenerator; // <-- ДОБАВЛЕНО

        public DebugController(
            ITaskDataConnection db, 
            ILogger<DebugController> logger,
            ReturnTaskGeneratorService returnTaskGenerator) // <-- ДОБАВЛЕНО В КОНСТРУКТОР
        {
            _db = db;
            _logger = logger;
            _returnTaskGenerator = returnTaskGenerator;
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
        /// ЧИТ-КОД 9: Мгновенная генерация задачи сборки заказа (OrderAssembly).
        /// Создает заказ, резервирует товар на полке в зоне STORAGE и выкидывает задачу в Общий пул.
        /// </summary>
        [HttpPost("tasks/generate-assembly")]
        public async Task<IActionResult> GenerateAssemblyTask([FromQuery] int branchId = 1)
        {
            _logger.LogInformation("|   [DEBUG] Генерация тестовой задачи сборки для филиала {BranchId}", branchId);
            var db = (DataConnection)_db;
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                // 1. Берем любой товар из базы
                var item = await db.GetTable<ItemModel>().FirstOrDefaultAsync();
                if (item == null) return BadRequest("Товары не найдены в БД.");

                // 2. Ищем полку для хранения (зона STORAGE)
                var storagePos = await db.GetTable<PositionModel>().FirstOrDefaultAsync(p => p.ZoneCode == "A" && p.BranchId == branchId)
                                 ?? await db.GetTable<PositionModel>().FirstAsync();

                // 3. Кладем товар на эту полку (создаем физический остаток)
                int itemPosId = await db.InsertWithInt32IdentityAsync(new ItemPositionModel
                {
                    ItemId = item.ItemId,
                    PositionId = storagePos.PositionId,
                    Quantity = 5, // Кладем с запасом
                    CreatedAt = DateTime.UtcNow
                });

                // 4. Создаем тестовый заказ
                int orderId = await db.InsertWithInt32IdentityAsync(new OrderModel
                {
                    CustomerId = 1,
                    BranchId = branchId,
                    Status = "Created",
                    DeliveryType = "Pickup",
                    PaymentType = "Prepaid",
                    DestinationAddress = "Тестовый адрес сборки",
                    CreatedAt = DateTime.UtcNow,
                    DeliveryDate = DateTime.UtcNow,
                    TotalPrice = 1500
                });

                // 5. Создаем позицию заказа
                int orderPosId = await db.InsertWithInt32IdentityAsync(new OrderPositionModel
                {
                    OrderId = orderId,
                    ItemId = item.ItemId,
                    Quantity = 2,
                    Price = 1500
                });

                // 6. Создаем резерв (связываем заказ с физической полкой)
                await db.InsertAsync(new OrderReservationModel
                {
                    OrderPositionId = orderPosId,
                    ItemPositionId = itemPosId,
                    Quantity = 2,
                    CreatedAt = DateTime.UtcNow
                });

                // 7. Создаем Базовую Задачу в системе
                int taskId = await db.InsertWithInt32IdentityAsync(new BaseTaskModel
                {
                    Title = $"Сборка заказа #{orderId}",
                    Type = "OrderAssembly",
                    Status = "New",
                    PriorityLevel = 1,
                    BranchId = branchId,
                    CreatedAt = DateTime.UtcNow
                });

                // 8. Создаем назначение (AssignedToUserId = 0, чтобы задача попала в Общий пул)
                int assignmentId = await db.InsertWithInt32IdentityAsync(new OrderAssemblyAssignmentModel
                {
                    TaskId = taskId,
                    OrderId = orderId,
                    AssignedToUserId = null,
                    BranchId = branchId,
                    Status = 0, // New
                    Complexity = 3.0,
                    Role = 1, // Основной сборщик
                    AssignedAt = DateTime.UtcNow
                });

                // 9. Создаем строку сборки (откуда брать и сколько)
                await db.InsertAsync(new OrderAssemblyLineModel
                {
                    OrderAssemblyAssignmentId = assignmentId,
                    ItemPositionId = itemPosId,
                    SourcePositionId = storagePos.PositionId,
                    TargetPositionId = null, // Целевую ячейку сотрудник отсканирует сам
                    Quantity = 2,
                    PickedQuantity = 0,
                    Status = 0 // Pending
                });

                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "Задача сборки успешно создана. Ищите её в 'Общем пуле' приложения.",
                    TaskId = taskId,
                    OrderId = orderId,
                    ItemName = item.Name,
                    CellCode = storagePos.FLSNumber // Подсказка, на какую полку идти
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "|   !!! [DEBUG] Ошибка в GenerateAssemblyTask");
                return StatusCode(500, ex.Message);
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
        /// ЧИТ-КОД 8: Мгновенная генерация кооперативной задачи выдачи (на 2 человека).
        /// Создает тяжелый заказ (50кг+) и два пустых назначения (Main и Helper) в общем пуле.
        /// </summary>
        [HttpPost("tasks/generate-cooperative-handover")]
        public async Task<IActionResult> GenerateCooperativeHandover([FromQuery] int branchId = 1)
        {
            _logger.LogInformation("|   [DEBUG] Генерация кооперативной задачи выдачи для филиала {BranchId}", branchId);
            var db = (DataConnection)_db;
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                // 1. Берем самый тяжелый товар из базы (эмулируем холодильник)
                var heavyItem = await db.GetTable<ItemModel>()
                    .OrderByDescending(i => i.Weight)
                    .FirstOrDefaultAsync();

                if (heavyItem == null) return BadRequest("Товары не найдены в БД.");
                // 2. Создаем тестовый заказ (с добавлением обязательных полей)
                int orderId = await db.InsertWithInt32IdentityAsync(new OrderModel
                {
                    CustomerId = 1,
                    BranchId = branchId,
                    Status = "Ready",
                    DeliveryType = "Pickup",
                    PaymentType = "Prepaid", // <-- ИСПРАВЛЕНИЕ: Добавлено обязательное поле
                    DestinationAddress = "Самовывоз со склада", // Тоже лучше заполнить
                    CreatedAt = DateTime.UtcNow,
                    DeliveryDate = DateTime.UtcNow,
                    TotalPrice = 50000
                });

                // 3. Создаем позицию заказа и физический остаток в зоне выдачи (PICKUP)
                var pickupPos = await db.GetTable<PositionModel>().FirstOrDefaultAsync(p => p.ZoneCode == "PICKUP")
                                ?? await db.GetTable<PositionModel>().FirstAsync();

                int itemPosId = await db.InsertWithInt32IdentityAsync(new ItemPositionModel
                {
                    ItemId = heavyItem.ItemId,
                    PositionId = pickupPos.PositionId,
                    Quantity = 1,
                    CreatedAt = DateTime.UtcNow
                });

                int orderPosId = await db.InsertWithInt32IdentityAsync(new OrderPositionModel
                {
                    OrderId = orderId,
                    ItemId = heavyItem.ItemId,
                    Quantity = 1,
                    Price = 50000
                });

                // 4. Резервируем товар
                await db.InsertAsync(new OrderReservationModel
                {
                    OrderPositionId = orderPosId,
                    ItemPositionId = itemPosId,
                    Quantity = 1,
                    CreatedAt = DateTime.UtcNow
                });

                // 5. Рассчитываем сложность через наш калькулятор
                // (Эмулируем параметры для тяжелого товара)
                double totalWeightKg = (double)heavyItem.Weight / 1000.0;

                // Используем DI для доступа к калькулятору или эмулируем логику 60/40
                double totalComplexity = 10.0 + (totalWeightKg * 0.1);
                double mainComplexity = Math.Round(totalComplexity * 0.6, 2);
                double helperComplexity = Math.Round(totalComplexity * 0.4, 2);

                // 6. Создаем Базовую Задачу
                int taskId = await db.InsertWithInt32IdentityAsync(new BaseTaskModel
                {
                    Title = $"[КООПЕРАЦИЯ] Выдача: {heavyItem.Name}",
                    Type = "OrderHandover",
                    Status = "New",
                    PriorityLevel = 5, // Высокий приоритет для тяжелых заказов
                    BranchId = branchId,
                    CreatedAt = DateTime.UtcNow
                });

                // 7. Создаем ДВА ПУСТЫХ назначения (Main и Helper)
                // Именно отсутствие AssignedToUserId заставит их отображаться в общем пуле
                int mainAssignmentId = await db.InsertWithInt32IdentityAsync(new OrderHandoverAssignmentModel
                {
                    TaskId = taskId,
                    OrderId = orderId,
                    HandoverType = "ToCustomer",
                    Status = 0, // New
                    Role = "Main",
                    Complexity = mainComplexity,
                    AssignedAt = DateTime.UtcNow,
                    AssignedToUserId = null // Вакансия
                });

                await db.InsertAsync(new OrderHandoverAssignmentModel
                {
                    TaskId = taskId,
                    OrderId = orderId,
                    HandoverType = "ToCustomer",
                    Status = 0, // New
                    Role = "Helper",
                    Complexity = helperComplexity,
                    AssignedAt = DateTime.UtcNow,
                    AssignedToUserId = null // Вакансия
                });

                // 8. Создаем строки выдачи для Main-назначения
                await db.InsertAsync(new OrderHandoverLineModel
                {
                    OrderHandoverAssignmentId = mainAssignmentId,
                    OrderPositionId = orderPosId,
                    ItemPositionId = itemPosId,
                    Quantity = 1,
                    ScannedQuantity = 0,
                    CancelledQuantity = 0
                });

                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "Кооперативная задача успешно создана. В общем пуле должны появиться назначения Main и Helper.",
                    TaskId = taskId,
                    OrderId = orderId,
                    ItemName = heavyItem.Name,
                    Weight = totalWeightKg
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "|   !!! [DEBUG] Ошибка в GenerateCooperativeHandover");
                return StatusCode(500, ex.Message);
            }
        }

        // ==========================================
        // ИЗМЕНЕННЫЙ ЭНДПОИНТ (ИСПОЛЬЗУЕТ СЕРВИС)
        // ==========================================
        [HttpPost("tasks/generate-return")]
        public async Task<IActionResult> GenerateReturnTask([FromQuery] int branchId = 1)
        {
            _logger.LogInformation("|   [DEBUG] Быстрая генерация задачи ReturnToStock для филиала {BranchId} через Сервис-Генератор", branchId);
            var db = (DataConnection)_db;
            
            int itemPosId = 0;
            ItemModel item = null;

            // ШАГ 1: Создаем фейковый остаток (эмуляция отказного товара в багажнике курьера)
            using (var transaction = await db.BeginTransactionAsync())
            {
                try
                {
                    // Берем случайный товар для теста
                    item = await db.GetTable<ItemModel>().OrderBy(_ => Guid.NewGuid()).FirstOrDefaultAsync();
                    if (item == null) return BadRequest("Товары не найдены в БД.");

                    // Находим ячейку-источник (PICKUP или COURIER)
                    var sourcePosition = await db.GetTable<PositionModel>()
                        .FirstOrDefaultAsync(p => p.ZoneCode == "PICKUP" || p.ZoneCode == "COURIER")
                        ?? await db.GetTable<PositionModel>().FirstAsync();

                    // Физический остаток в ячейке
                    itemPosId = await db.InsertWithInt32IdentityAsync(new ItemPositionModel
                    {
                        ItemId = item.ItemId,
                        PositionId = sourcePosition.PositionId,
                        Quantity = 1,
                        CreatedAt = DateTime.UtcNow
                    });

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Ошибка при подготовке тестовых данных: {ex.Message}");
                }
            }

            // ШАГ 2: Формируем список для генератора
            var itemsToReturn = new List<(int ItemPositionId, int Qty)> { (itemPosId, 1) };

            // ШАГ 3: Вызываем сервис! Он сам посчитает вес, сложность и НАЙДЕТ помощника
            int? taskId = await _returnTaskGenerator.GenerateReturnTaskForCourierAsync(999, branchId, itemsToReturn);

            return Ok(new 
            { 
                Message = "Задача возврата успешно создана через ReturnTaskGeneratorService", 
                TaskId = taskId,
                ItemName = item?.Name,
                Weight = item?.Weight
            });
        }
        // ==========================================


        [HttpGet("cell/{positionId}/inspect")]
        public async Task<IActionResult> InspectCell(int positionId)
        {
            var pos = await _db.GetTable<PositionModel>().FirstOrDefaultAsync(p => p.PositionId == positionId);
            var items = await _db.GetTable<ItemPositionModel>().Where(ip => ip.PositionId == positionId).ToListAsync();

            // Логируем для отладки "зависших" статусов
            _logger.LogInformation("|   [DEBUG] Инспекция ячейки {Id}: Статус={Status}, Кол-во записей={Count}",
                positionId, pos?.Status, items.Count);

            return Ok(new
            {
                CellId = positionId,
                Status = pos?.Status,
                Items = items.Select(i => new { i.Id, i.ItemId, i.Quantity })
            });
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
                    return_assignments,
                    base_tasks,
                    return_lines,
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
    }
}
#endif