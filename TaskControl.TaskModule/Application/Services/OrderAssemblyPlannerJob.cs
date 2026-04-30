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
// Подключаем модели из других модулей:
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.OrderModule.Domain;

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

        public OrderAssemblyPlannerJob(
            ITaskDataConnection db,
            IBoxPackingService packingService,
            ILogger<OrderAssemblyPlannerJob> logger,
            IOptions<AppSettings> appSettings,
            TaskWorkloadAggregator taskWorkloadAggregator,
            IBaseTaskService baseTaskService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _packingService = packingService ?? throw new ArgumentNullException(nameof(packingService));
            _logger = logger;
            _appSettings = appSettings?.Value ?? new AppSettings();
            _baseTaskService = baseTaskService ?? throw new ArgumentNullException(nameof(baseTaskService));
            _taskWorkloadAggregator = taskWorkloadAggregator ?? throw new ArgumentNullException(nameof(taskWorkloadAggregator));
        }

        public async Task ExecuteAsync()
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("Запуск OrderAssemblyPlannerJob (планирование сборок заказов)");

            try
            {
                var conn = (DataConnection)_db;
                using var transaction = await conn.BeginTransactionAsync();

                var targetTime = DateTime.UtcNow.AddHours(2);

                // Получаем подходящие заказы (исключаем постаматы, так как они пакуются при создании)
                var targetOrders = await _db.GetTable<OrderModel>()
                    .Where(o => o.Status == "Created" && o.DeliveryDate <= targetTime && o.DeliveryType != "Postamat")
                    .Select(o => new { o.OrderId, o.BranchId, o.DeliveryDate })
                    .ToListAsync();

                _logger.LogInformation("+--- Планирование сборки заказов: найдено {Count} заказов (до {TargetTime:yyyy-MM-dd HH:mm})",
                    targetOrders.Count, targetTime);

                var plannedCount = 0;
                var skippedCount = 0;

                foreach (var order in targetOrders)
                {
                    _logger.LogInformation("|\n| [Заказ #{OrderId}] филиал {BranchId}", order.OrderId, order.BranchId);

                    bool success = await ProcessOrderInternal(order.OrderId, order.BranchId, order.DeliveryDate);

                    if (success)
                        plannedCount++;
                    else
                        skippedCount++;
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

        public async Task PlanSingleOrderAsync(int orderId)
        {
            _logger.LogInformation(">>> СРОЧНОЕ ПЛАНИРОВАНИЕ (Hangfire): Заказ #{OrderId}", orderId);

            try
            {
                var conn = (DataConnection)_db;
                using var transaction = await conn.BeginTransactionAsync();

                var order = await _db.GetTable<OrderModel>()
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.Status == DeliveryType.Express.ToString());

                if (order == null)
                {
                    _logger.LogWarning("Заказ #{OrderId} не найден в БД", orderId);
                    return;
                }

                bool success = await ProcessOrderInternal(order.OrderId, order.BranchId, order.DeliveryDate);

                if (success)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation(">>> Срочное планирование заказа #{OrderId} успешно завершено", orderId);
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(">>> Срочное планирование заказа #{OrderId} пропущено (нехватка ячеек/остатков)", orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при срочном планировании заказа #{OrderId}", orderId);
                throw;
            }
        }

        private async Task<bool> ProcessOrderInternal(int orderId, int branchId, DateTime? deliveryDate)
        {
            // 1. Получаем позиции для сборки (с весом из ItemModel)
            // Жесткая резервация (ItemPositionId != null) уже была выполнена при создании заказа.
            var orderItems = await (from op in _db.GetTable<OrderPositionModel>()
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
                                        Weight = i.Weight, // Вес для скоринга и детекции тяжести
                                        SourcePositionId = p.PositionId,
                                        Quantity = ores.Quantity
                                    }).ToListAsync();

            if (orderItems.Count == 0) return false;

            // 2. Анализ тяжести и расчет сложности
            var heavyItems = orderItems.Where(x => x.Weight >= _appSettings.MaxWeightPerWorker).ToList();
            bool needsHelper = heavyItems.Any();

            double mainWeight = orderItems.Where(x => x.Weight < _appSettings.MaxWeightPerWorker).Sum(x => x.Weight * x.Quantity)
                                + (heavyItems.Sum(x => x.Weight * x.Quantity) / 2);
            double helperWeight = heavyItems.Sum(x => x.Weight * x.Quantity) / 2;

            double mainComplexity = 1.0 + (mainWeight * _appSettings.WeightCoefficient);
            double helperComplexity = needsHelper ? (0.5 + (helperWeight * _appSettings.WeightCoefficient)) : 0;

            // 3. Планирование ячеек (PICKUP)
            var itemsToPack = orderItems.Select(x => new ItemToPack
            {
                OrderPositionId = x.OrderPositionId,
                ItemId = x.ItemId,
                Length = x.Length,
                Width = x.Width,
                Height = x.Height,
                Quantity = x.Quantity
            }).ToList();

            var occupiedCellIds = _db.GetTable<ItemPositionModel>().Select(ip => ip.PositionId);
            var availableCells = await _db.GetTable<PositionModel>()
                .Where(p => p.BranchId == branchId && p.ZoneCode == "PICKUP" && p.Status == "Active" && !occupiedCellIds.Contains(p.PositionId))
                .Select(p => new CellToPackInto { PositionId = p.PositionId, Length = p.Length, Width = p.Width, Height = p.Height })
                .ToListAsync();

            var packingResult = _packingService.AssignItemsToPickupCells(itemsToPack, availableCells);
            if (!packingResult.IsFullyPacked)
            {
                return false;
            }

            // 4. Подбор персонала через Агрегатор
            var workers = await _baseTaskService.GetAutoSelectedEmployeesAsync(branchId, int.MaxValue);
            var workerScores = new Dictionary<int, double>();
            foreach (var w in workers)
            {
                workerScores[w] = await _taskWorkloadAggregator.GetTotalActiveComplexityAsync(w); // Учет всех модулей
            }
            var sortedWorkers = workers.OrderBy(w => workerScores[w]).ToList();

            int workersNeeded = needsHelper ? 2 : 1;
            if (sortedWorkers.Count < workersNeeded)
            {
                return false;
            }

            // 5. Создание задачи и назначений (с учетом Complexity и Role)
            var baseTask = new BaseTaskModel
            {
                Title = $"Сборка заказа {orderId}",
                BranchId = branchId,
                Type = "OrderAssembly",
                Status = "New",
                PriorityLevel = 1,
                CreatedAt = DateTime.UtcNow,
                Deadline = deliveryDate
            };
            var taskId = await _db.InsertWithInt32IdentityAsync(baseTask);

            // Основной работник
            var mainAssignment = new OrderAssemblyAssignmentModel
            {
                TaskId = taskId,
                OrderId = orderId,
                AssignedToUserId = sortedWorkers[0],
                BranchId = branchId,
                Status = 0,
                Complexity = mainComplexity,
                Role = (int)AssignmentRole.Main,
                AssignedAt = DateTime.UtcNow
            };
            mainAssignment.Id = await _db.InsertWithInt32IdentityAsync(mainAssignment);

            // Помощник (если есть тяжелые товары)
            OrderAssemblyAssignmentModel helperAssignment = null;
            if (needsHelper)
            {
                helperAssignment = new OrderAssemblyAssignmentModel
                {
                    TaskId = taskId,
                    OrderId = orderId,
                    AssignedToUserId = sortedWorkers[1],
                    BranchId = branchId,
                    Status = 0,
                    Complexity = helperComplexity,
                    Role = (int)AssignmentRole.Helper,
                    AssignedAt = DateTime.UtcNow
                };
                helperAssignment.Id = await _db.InsertWithInt32IdentityAsync(helperAssignment);
            }

            // 6. Создание строк сборки (Helper получает только тяжелые товары)
            foreach (var packedBlock in packingResult.PackedItems)
            {
                var original = orderItems.First(o => o.OrderPositionId == packedBlock.OrderPositionId);
                bool isHeavy = original.Weight >= _appSettings.MaxWeightPerWorker;

                // Основному - всё
                await _db.InsertAsync(new OrderAssemblyLineModel
                {
                    OrderAssemblyAssignmentId = mainAssignment.Id,
                    ItemPositionId = original.ItemPositionId,
                    SourcePositionId = original.SourcePositionId,
                    TargetPositionId = packedBlock.TargetPositionId,
                    Quantity = packedBlock.Quantity,
                    Status = 0
                });

                // Помощнику - только тяжелое
                if (isHeavy && helperAssignment != null)
                {
                    await _db.InsertAsync(new OrderAssemblyLineModel
                    {
                        OrderAssemblyAssignmentId = helperAssignment.Id,
                        ItemPositionId = original.ItemPositionId,
                        SourcePositionId = original.SourcePositionId,
                        TargetPositionId = packedBlock.TargetPositionId,
                        Quantity = packedBlock.Quantity,
                        Status = 0
                    });
                }
            }

            await _db.GetTable<OrderModel>().Where(o => o.OrderId == orderId).Set(o => o.Status, "Assembly").UpdateAsync();
            return true;
        }
        private async Task RollbackHardAllocationAsync(int orderId, List<SoftReservation> originalSoftReservations)
        {
            if (originalSoftReservations == null || !originalSoftReservations.Any()) return;

            _logger.LogWarning("|   ! Откат Hard Allocation для заказа {OrderId}", orderId);

            // 1. Получаем все уникальные ID позиций этого заказа
            var orderPositionIds = _db.GetTable<OrderPositionModel>()
                                      .Where(op => op.OrderId == orderId)
                                      .Select(op => op.UniqueId);

            // 2. Удаляем все текущие резервы (включая разбитые)
            await _db.GetTable<OrderReservationModel>()
                     .Where(r => orderPositionIds.Contains(r.OrderPositionId))
                     .DeleteAsync();

            // 3. Восстанавливаем оригинальные "мягкие" резервы
            foreach (var orig in originalSoftReservations)
            {
                await _db.InsertAsync(new OrderReservationModel
                {
                    OrderPositionId = orig.OrderPositionId,
                    ItemPositionId = null, // Снова мягкий резерв
                    Quantity = orig.Quantity,
                    CreatedAt = DateTime.UtcNow
                });
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

        private class SoftReservation
        {
            public int ReservationId { get; set; }
            public int ItemId { get; set; }
            public int Quantity { get; set; }
            public int OrderPositionId { get; set; }
        }
    }
}