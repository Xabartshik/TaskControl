using Hangfire.Server;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.OrderModule.Domain;
using TaskControl.TaskModule.Application.Helpers;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services
{
    public class OrderAssemblyPlannerJob
    {
        private readonly ITaskDataConnection _db;
        private readonly IBoxPackingService _packingService;
        private readonly ILogger<OrderAssemblyPlannerJob> _logger;
        private readonly AppSettings _appSettings;
        private readonly TaskWorkloadAggregator _taskWorkloadAggregator;
        private readonly IBaseTaskService _baseTaskService;
        private readonly INotificationService _notificationService;

        public OrderAssemblyPlannerJob(
            ITaskDataConnection db,
            IBoxPackingService packingService,
            ILogger<OrderAssemblyPlannerJob> logger,
            IOptions<AppSettings> appSettings,
            TaskWorkloadAggregator taskWorkloadAggregator,
            INotificationService notificationService,
            IBaseTaskService baseTaskService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _packingService = packingService ?? throw new ArgumentNullException(nameof(packingService));
            _logger = logger;
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _appSettings = appSettings?.Value ?? new AppSettings();
            _baseTaskService = baseTaskService ?? throw new ArgumentNullException(nameof(baseTaskService));
            _taskWorkloadAggregator = taskWorkloadAggregator ?? throw new ArgumentNullException(nameof(taskWorkloadAggregator));
        }

        private DateTime CalculateDeadline(OrderModel order)
        {
            if (order.DeliveryType == DeliveryType.Express.ToString())
                return DateTime.UtcNow;

            if (order.DeliveryType == DeliveryType.Pickup.ToString())
                return (order.DeliveryDate ?? DateTime.UtcNow.AddHours(2)).AddHours(-2);

            if (order.DeliveryType == DeliveryType.Delivery.ToString() && order.DeliverySlotId.HasValue)
            {
                var slot = DeliverySchedule.Slots.FirstOrDefault(s => s.Id == order.DeliverySlotId.Value);
                if (slot != null && order.DeliveryDate.HasValue)
                {
                    var slotStart = order.DeliveryDate.Value.Date.Add(slot.StartTime);
                    return slotStart.AddHours(-2);
                }
            }

            if (order.DeliveryType == DeliveryType.Postamat.ToString())
            {
                var targetDate = order.CreatedAt.AddDays(2);
                var targetTimeOfDay = targetDate.TimeOfDay;

                var nextSlot = DeliverySchedule.Slots
                    .OrderBy(s => s.StartTime)
                    .FirstOrDefault(s => s.StartTime > targetTimeOfDay);

                if (nextSlot == null)
                {
                    nextSlot = DeliverySchedule.Slots.OrderBy(s => s.StartTime).First();
                    targetDate = targetDate.AddDays(1);
                }

                var slotStart = targetDate.Date.Add(nextSlot.StartTime);
                return slotStart.AddHours(-2);
            }

            return order.DeliveryDate ?? DateTime.UtcNow.AddHours(2);
        }

        public async Task ExecuteAsync()
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("Запуск OrderAssemblyPlannerJob (планирование сборок заказов)");

            try
            {
                var conn = (DataConnection)_db;
                using var transaction = await conn.BeginTransactionAsync();

                var searchLimit = DateTime.UtcNow.AddDays(2);

                var allCreatedOrders = await _db.GetTable<OrderModel>()
                    .Where(o => o.Status == "Created")
                    .ToListAsync();

                var targetOrders = allCreatedOrders
                    .Select(o => new { Order = o, Deadline = CalculateDeadline(o) })
                    .Where(x => x.Deadline <= searchLimit)
                    .ToList();

                _logger.LogInformation("+--- Планирование сборки заказов: найдено {Count} заказов (до {SearchLimit:yyyy-MM-dd HH:mm})",
                    targetOrders.Count, searchLimit);

                var plannedCount = 0;
                var skippedCount = 0;

                foreach (var item in targetOrders)
                {
                    bool success = await ProcessOrderInternal(item.Order, item.Deadline, null, false);

                    if (success) plannedCount++;
                    else skippedCount++;
                }

                await transaction.CommitAsync();

                _logger.LogInformation("=== Итог планирования: успешно спланировано {Planned}, пропущено {Skipped} из {Total} заказов ===",
                    plannedCount, skippedCount, targetOrders.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выполнении OrderAssemblyPlannerJob");
                throw;
            }
        }

        public async Task AssignSelfToOrderAsync(int orderId, int workerId)
        {
            _logger.LogInformation(">>> РУЧНОЕ НАЗНАЧЕНИЕ: Сотрудник {WorkerId} берет заказ #{OrderId}", workerId, orderId);

            try
            {
                var conn = (DataConnection)_db;
                using var transaction = await conn.BeginTransactionAsync();

                var order = await _db.GetTable<OrderModel>()
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.Status == "Created");

                if (order == null)
                    throw new InvalidOperationException("Заказ не найден, отменен или уже находится в сборке.");

                var deadline = CalculateDeadline(order);

                bool success = await ProcessOrderInternal(order, deadline, workerId, throwOnWorkerShortage: true);

                if (success)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation(">>> Сотрудник {WorkerId} успешно назначен на заказ #{OrderId}", workerId, orderId);
                }
                else
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("Не удалось сформировать задачу сборки.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ручном назначении сотрудника {WorkerId} на заказ #{OrderId}", workerId, orderId);
                throw;
            }
        }

        public async Task PlanSingleOrderAsync(int orderId)
        {
            _logger.LogInformation(">>> СРОЧНОЕ ПЛАНИРОВАНИЕ (Hangfire): Заказ #{OrderId}", orderId);

            try
            {
                var conn = (DataConnection)_db;
                using var transaction = await conn.BeginTransactionAsync();

                var order = await _db.GetTable<OrderModel>()
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.Status == "Created");

                if (order == null)
                {
                    _logger.LogWarning("Заказ #{OrderId} не найден в БД или уже в работе", orderId);
                    return;
                }

                var deadline = CalculateDeadline(order);
                bool success = await ProcessOrderInternal(order, deadline, null, false);

                if (success)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation(">>> Срочное планирование заказа #{OrderId} успешно завершено", orderId);
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(">>> Срочное планирование заказа #{OrderId} пропущено", orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при срочном планировании заказа #{OrderId}", orderId);
                throw;
            }
        }

        // =========================================================================================
        // РЕФАКТОРИНГ: ОСНОВНОЙ МЕТОД ОРКЕСТРАЦИИ 
        // =========================================================================================
        private async Task<bool> ProcessOrderInternal(OrderModel order, DateTime calculatedDeadline, int? manualWorkerId, bool throwOnWorkerShortage)
        {
            // 1. Получение товаров
            var orderItems = await GetOrderItemsAsync(order.OrderId);
            if (orderItems.Count == 0)
            {
                _logger.LogWarning("|   [Заказ #{OrderId}] Пропуск: не найдены позиции для сборки.", order.OrderId);
                return false;
            }

            int bulkCellId = await GetActiveZoneCellIdAsync(order.BranchId, "BULK");
            if (bulkCellId == 0)
            {
                _logger.LogError("|   [Заказ #{OrderId}] Критическая ошибка: нет зоны BULK.", order.OrderId);
                if (throwOnWorkerShortage) throw new InvalidOperationException("В филиале не настроена зона BULK.");
                return false;
            }

            // 2. Расчет сложности (с помощником или без)
            var (needsHelper, mainComplexity, helperComplexity) = CalculateWorkloadComplexity(orderItems);

            // 3. Маршрутизация упаковки (Где будут лежать товары?)
            var finalPackedItems = await RouteItemsForPackingAsync(order, orderItems, bulkCellId);

            // 4. Подбор персонала
            var sortedWorkers = await AssignWorkersAsync(order.BranchId, needsHelper, manualWorkerId, throwOnWorkerShortage);
            if (sortedWorkers == null) return false;        

            // 5. Создание задачи в БД
            TaskPriority basePriority = order.DeliveryType == DeliveryType.Express.ToString() ? TaskPriority.Critical : TaskPriority.Normal;
            TaskPriority finalPriority = PriorityCalculator.CalculatePriority(calculatedDeadline, basePriority);
            string taskTitle = $"Сборка заказа {order.OrderId}";

            var taskId = await CreateTaskAndAssignmentsAsync(
                order, calculatedDeadline, finalPriority, taskTitle,
                mainComplexity, helperComplexity, needsHelper, sortedWorkers, finalPackedItems);

            // 6. Уведомления и статусы
            await SendNotificationsAsync(order.OrderId, taskId, taskTitle, finalPriority, sortedWorkers, needsHelper, manualWorkerId.HasValue);
            await _db.GetTable<OrderModel>().Where(o => o.OrderId == order.OrderId).Set(o => o.Status, "Assembly").UpdateAsync();

            _logger.LogInformation("|   [Заказ #{OrderId}] Успешно спланирован. Приоритет: {Priority}, Дедлайн: {Deadline}", order.OrderId, finalPriority, calculatedDeadline);
            return true;
        }


        // =========================================================================================
        // ПРИВАТНЫЕ МЕТОДЫ (ПОДЗАДАЧИ)
        // =========================================================================================

        private async Task<List<RawItemToPack>> GetOrderItemsAsync(int orderId)
        {
            return await (from op in _db.GetTable<OrderPositionModel>()
                          join ores in _db.GetTable<OrderReservationModel>() on op.UniqueId equals ores.OrderPositionId
                          join ip in _db.GetTable<ItemPositionModel>() on ores.ItemPositionId equals ip.Id
                          join i in _db.GetTable<ItemModel>() on ip.ItemId equals i.ItemId
                          join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                          where op.OrderId == orderId
                          select new RawItemToPack
                          {
                              OrderPositionId = op.UniqueId,
                              ItemId = ip.ItemId,
                              ItemPositionId = ip.Id,
                              Length = i.Length,
                              Width = i.Width,
                              Height = i.Height,
                              Weight = i.Weight,
                              SourcePositionId = p.PositionId,
                              Quantity = ores.Quantity
                          }).ToListAsync();
        }

        private async Task<int> GetActiveZoneCellIdAsync(int branchId, params string[] zoneCodes)
        {
            return await _db.GetTable<PositionModel>()
                .Where(p => p.BranchId == branchId && zoneCodes.Contains(p.ZoneCode) && p.Status == "Active")
                .Select(p => p.PositionId)
                .FirstOrDefaultAsync();
        }

        private (bool NeedsHelper, double MainComplexity, double HelperComplexity) CalculateWorkloadComplexity(List<RawItemToPack> orderItems)
        {
            var heavyItems = orderItems.Where(x => x.Weight >= _appSettings.MaxWeightPerWorker).ToList();
            bool needsHelper = heavyItems.Any();

            double mainWeight = orderItems.Where(x => x.Weight < _appSettings.MaxWeightPerWorker).Sum(x => x.Weight * x.Quantity)
                                + (heavyItems.Sum(x => x.Weight * x.Quantity) / 2);
            double helperWeight = heavyItems.Sum(x => x.Weight * x.Quantity) / 2;

            double mainComplexity = 1.0 + (mainWeight * _appSettings.WeightCoefficient);
            double helperComplexity = needsHelper ? (0.5 + (helperWeight * _appSettings.WeightCoefficient)) : 0;

            return (needsHelper, mainComplexity, helperComplexity);
        }

        private async Task<List<PackedItemResult>> RouteItemsForPackingAsync(OrderModel order, List<RawItemToPack> orderItems, int bulkCellId)
        {
            var finalPackedItems = new List<PackedItemResult>();

            if (order.DeliveryType == DeliveryType.Express.ToString())
            {
                var issueCellId = await GetActiveZoneCellIdAsync(order.BranchId, "ISSUE", "EXPRESS");
                if (issueCellId == 0) issueCellId = bulkCellId;

                finalPackedItems.AddRange(orderItems.Select(item => new PackedItemResult
                {
                    OrderPositionId = item.OrderPositionId,
                    TargetPositionId = issueCellId,
                    Quantity = item.Quantity
                }));
            }
            else
            {
                var itemsToPack = orderItems.Select(x => new ItemToPack
                {
                    OrderPositionId = x.OrderPositionId,
                    ItemId = x.ItemId,
                    Length = x.Length,
                    Width = x.Width,
                    Height = x.Height,
                    Weight = x.Weight,
                    Quantity = x.Quantity
                }).ToList();

                var occupiedCellIds = _db.GetTable<ItemPositionModel>().Select(ip => ip.PositionId);
                var availableCells = await _db.GetTable<PositionModel>()
                    .Where(p => p.BranchId == order.BranchId && p.ZoneCode == "PICKUP" && p.Status == "Active" && !occupiedCellIds.Contains(p.PositionId))
                    .Select(p => new CellToPackInto { PositionId = p.PositionId, Length = p.Length, Width = p.Width, Height = p.Height })
                    .ToListAsync();

                var packingResult = _packingService.AssignItemsToPickupCells(itemsToPack, availableCells);
                finalPackedItems.AddRange(packingResult.PackedItems);

                if (!packingResult.IsFullyPacked)
                {
                    foreach (var originalItem in orderItems)
                    {
                        int packedQty = packingResult.PackedItems.Where(x => x.OrderPositionId == originalItem.OrderPositionId).Sum(x => x.Quantity);
                        int remains = originalItem.Quantity - packedQty;
                        if (remains > 0)
                        {
                            finalPackedItems.Add(new PackedItemResult
                            {
                                OrderPositionId = originalItem.OrderPositionId,
                                TargetPositionId = bulkCellId,
                                Quantity = remains
                            });
                        }
                    }
                }
            }
            return finalPackedItems;
        }

        private async Task<List<int>> AssignWorkersAsync(int branchId, bool needsHelper, int? manualWorkerId, bool throwOnWorkerShortage)
        {
            var sortedWorkers = new List<int>();
            if (manualWorkerId.HasValue) sortedWorkers.Add(manualWorkerId.Value);

            int workersToAutoFind = needsHelper
                ? (manualWorkerId.HasValue ? 1 : 2)
                : (manualWorkerId.HasValue ? 0 : 1);

            if (workersToAutoFind > 0)
            {
                var autoWorkers = await _baseTaskService.GetAutoSelectedEmployeesAsync(branchId, workersToAutoFind);
                if (autoWorkers.Count() < workersToAutoFind)
                {
                    if (throwOnWorkerShortage) throw new InvalidOperationException("Невозможно взять заказ: требуется напарник, но нет свободных сотрудников.");
                    return null;
                }

                var workerScores = new Dictionary<int, double>();
                foreach (var w in autoWorkers)
                    workerScores[w] = await _taskWorkloadAggregator.GetTotalActiveComplexityAsync(w);

                sortedWorkers.AddRange(autoWorkers.OrderBy(w => workerScores[w]));
            }

            return sortedWorkers;
        }

        private async Task<int> CreateTaskAndAssignmentsAsync(
            OrderModel order, DateTime deadline, TaskPriority priority, string taskTitle,
            double mainComplexity, double helperComplexity, bool needsHelper,
            List<int> sortedWorkers, List<PackedItemResult> packedItems)
        {
            var baseTask = new BaseTaskModel
            {
                Title = taskTitle,
                BranchId = order.BranchId,
                Type = "OrderAssembly",
                Status = "New",
                PriorityLevel = (int)priority,
                CreatedAt = DateTime.UtcNow,
                Deadline = deadline
            };
            var taskId = await _db.InsertWithInt32IdentityAsync(baseTask);

            var mainAssignmentId = await _db.InsertWithInt32IdentityAsync(new OrderAssemblyAssignmentModel
            {
                TaskId = taskId,
                OrderId = order.OrderId,
                AssignedToUserId = sortedWorkers[0],
                BranchId = order.BranchId,
                Status = 0,
                Complexity = mainComplexity,
                Role = (int)AssignmentRole.Main,
                AssignedAt = DateTime.UtcNow
            });

            if (needsHelper)
            {
                await _db.InsertAsync(new OrderAssemblyAssignmentModel
                {
                    TaskId = taskId,
                    OrderId = order.OrderId,
                    AssignedToUserId = sortedWorkers[1],
                    BranchId = order.BranchId,
                    Status = 0,
                    Complexity = helperComplexity,
                    Role = (int)AssignmentRole.Helper,
                    AssignedAt = DateTime.UtcNow
                });
            }

            // Достаем оригинальные позиции для связи с Target
            var orderItems = await GetOrderItemsAsync(order.OrderId);

            foreach (var packedBlock in packedItems)
            {
                var original = orderItems.First(o => o.OrderPositionId == packedBlock.OrderPositionId);
                await _db.InsertAsync(new OrderAssemblyLineModel
                {
                    OrderAssemblyAssignmentId = mainAssignmentId,
                    ItemPositionId = original.ItemPositionId,
                    SourcePositionId = original.SourcePositionId,
                    TargetPositionId = packedBlock.TargetPositionId,
                    Quantity = packedBlock.Quantity,
                    Status = 0
                });
            }

            return taskId;
        }

        private async Task SendNotificationsAsync(int orderId, int taskId, string taskTitle, TaskPriority priority, List<int> sortedWorkers, bool hasHelper, bool isManualAssignment)
        {
            if (hasHelper)
            {
                await _notificationService.NotifyHelperRequiredAsync(sortedWorkers[1], orderId);
            }

            if (!isManualAssignment)
            {
                if (priority == TaskPriority.Critical || priority == TaskPriority.High)
                    await _notificationService.NotifyHighPriorityTaskAsync(sortedWorkers[0], taskId, taskTitle);
                else
                    await _notificationService.NotifyNewTaskAsync(sortedWorkers[0], taskId, taskTitle);
            }
        }

        private class RawItemToPack
        {
            public int OrderPositionId { get; set; }
            public int ItemId { get; set; }
            public int ItemPositionId { get; set; }
            public double Length { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public double Weight { get; set; }
            public int SourcePositionId { get; set; }
            public int Quantity { get; set; }
        }
    }
}