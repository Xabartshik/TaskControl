using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
// Подключаем модели из других модулей:
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.Application.Services
{
    public class OrderAssemblyPlannerJob
    {
        private readonly ITaskDataConnection _db;
        private readonly IBoxPackingService _packingService;
        private readonly ILogger<OrderAssemblyPlannerJob> _logger;
        private readonly AppSettings _appSettings;
        private readonly IBaseTaskService _baseTaskService;

        public OrderAssemblyPlannerJob(
            ITaskDataConnection db,
            IBoxPackingService packingService,
            ILogger<OrderAssemblyPlannerJob> logger,
            IOptions<AppSettings> appSettings,
            IBaseTaskService baseTaskService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _packingService = packingService ?? throw new ArgumentNullException(nameof(packingService));
            _logger = logger;
            _appSettings = appSettings?.Value ?? new AppSettings();
            _baseTaskService = baseTaskService ?? throw new ArgumentNullException(nameof(baseTaskService));
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

                // Получаем подходящие заказы
                var targetOrders = await _db.GetTable<OrderModel>()
                    .Where(o => o.Status == "New" && o.DeliveryDate <= targetTime)
                    .Select(o => new { o.OrderId, o.BranchId, o.DeliveryDate })
                    .ToListAsync();

                _logger.LogInformation("+--- Планирование сборки заказов: найдено {Count} заказов (до {TargetTime:yyyy-MM-dd HH:mm})",
                    targetOrders.Count, targetTime);

                var plannedCount = 0;
                var skippedCount = 0;

                foreach (var order in targetOrders)
                {
                    _logger.LogInformation("|\n| [Заказ #{OrderId}] филиал {BranchId}", order.OrderId, order.BranchId);
                    // 1. HARD ALLOCATION: Перевод "мягких" резервов в "жесткие"
                    var softReservations = await (from ores in _db.GetTable<OrderReservationModel>()
                                                  join op in _db.GetTable<OrderPositionModel>() on ores.OrderPositionId equals op.UniqueId
                                                  where op.OrderId == order.OrderId && ores.ItemPositionId == null
                                                  select new SoftReservation
                                                  {
                                                      ReservationId = ores.Id,
                                                      ItemId = op.ItemId,
                                                      Quantity = ores.Quantity,
                                                      OrderPositionId = op.UniqueId
                                                  }).ToListAsync();

                    if (softReservations.Any())
                    {
                        _logger.LogInformation("|   > Запуск Hard Allocation для {Count} позиций...", softReservations.Count);
                        bool hardAllocationFailed = false;

                        foreach (var sRes in softReservations)
                        {
                            int neededQty = sRes.Quantity;

                            // Ищем свободные остатки (Вычитаем зарезервированные количества)
                            var stocksQuery = from ip in _db.GetTable<ItemPositionModel>()
                                              join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                                              where ip.ItemId == sRes.ItemId && p.BranchId == order.BranchId
                                              let reservedQty = _db.GetTable<OrderReservationModel>()
                                                                   .Where(r => r.ItemPositionId == ip.Id)
                                                                   .Sum(r => (int?)r.Quantity) ?? 0
                                              let availableQty = ip.Quantity - reservedQty
                                              where availableQty > 0
                                              select new
                                              {
                                                  ItemPositionId = ip.Id,
                                                  AvailableQty = availableQty
                                              };

                            var stocks = await stocksQuery.ToListAsync();

                            bool isFirst = true;
                            foreach (var stock in stocks)
                            {
                                if (neededQty <= 0) break;

                                int takeQty = Math.Min(neededQty, stock.AvailableQty);

                                if (isFirst)
                                {
                                    // Обновляем текущую запись (привязываем ячейку)
                                    await _db.GetTable<OrderReservationModel>()
                                             .Where(r => r.Id == sRes.ReservationId)
                                             .Set(r => r.ItemPositionId, stock.ItemPositionId)
                                             .Set(r => r.Quantity, takeQty)
                                             .UpdateAsync();
                                    isFirst = false;
                                }
                                else
                                {
                                    await _db.InsertAsync(new OrderReservationModel
                                    {
                                        OrderPositionId = sRes.OrderPositionId,
                                        ItemPositionId = stock.ItemPositionId,
                                        Quantity = takeQty,
                                        CreatedAt = DateTime.UtcNow
                                    });
                                }

                                neededQty -= takeQty;
                            }

                            if (neededQty > 0)
                            {
                                _logger.LogWarning("|   ! недостаточно товара на полках для перевода в Hard Allocation (ItemId: {ItemId})", sRes.ItemId);
                                hardAllocationFailed = true;
                                break;
                            }
                        }

                        if (hardAllocationFailed)
                        {
                            await RollbackHardAllocationAsync(order.OrderId, softReservations);
                            skippedCount++;
                            continue;
                        }
                    }
                    // 2. Получаем позиции для сборки (с физическими координатами)
                    var orderItems = await (from op in _db.GetTable<OrderPositionModel>()
                                            join ores in _db.GetTable<OrderReservationModel>() on op.UniqueId equals ores.OrderPositionId
                                            join ip in _db.GetTable<ItemPositionModel>() on ores.ItemPositionId equals ip.Id
                                            join i in _db.GetTable<ItemModel>() on ip.ItemId equals i.ItemId
                                            join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                                            where op.OrderId == order.OrderId
                                            select new RawItemToPack
                                            {
                                                OrderPositionId = op.UniqueId,
                                                ItemId = ip.ItemId,
                                                ItemPositionId = ip.Id,
                                                Length = i.Length,
                                                Width = i.Width,
                                                Height = i.Height,
                                                SourcePositionId = p.PositionId,
                                                Quantity = ores.Quantity
                                            }).ToListAsync();

                    if (orderItems.Count == 0)
                    {
                        _logger.LogWarning("|   ! нет позиций для сборки, пропуск");
                        skippedCount++;
                        continue;
                    }

                    _logger.LogInformation("|   > найдено {Count} позиций для сборки", orderItems.Count);

                    var itemsToPack = orderItems.Select(x => new ItemToPack
                    {
                        OrderPositionId = x.OrderPositionId,
                        ItemId = x.ItemId,
                        Length = x.Length,
                        Width = x.Width,
                        Height = x.Height,
                        Quantity = x.Quantity
                    }).ToList();

                    // Ищем свободные ячейки PICKUP
                    var occupiedCellIds = _db.GetTable<ItemPositionModel>().Select(ip => ip.PositionId);
                    var availableCells = await _db.GetTable<PositionModel>()
                        .Where(p => p.BranchId == order.BranchId
                                 && p.ZoneCode == "PICKUP"
                                 && p.Status == "Active"
                                 && !occupiedCellIds.Contains(p.PositionId))
                        .Select(p => new CellToPackInto
                        {
                            PositionId = p.PositionId,
                            Length = p.Length,
                            Width = p.Width,
                            Height = p.Height
                        }).ToListAsync();

                    // Распределяем товары по ячейкам PICKUP
                    var packingResult = _packingService.AssignItemsToPickupCells(itemsToPack, availableCells);

                    // Проверяем флаг IsFullyPacked
                    if (!packingResult.IsFullyPacked)
                    {
                        _logger.LogWarning("|   ! не удалось распланировать весь заказ: недостаточно объема в свободных ячейках PICKUP");
                        await RollbackHardAllocationAsync(order.OrderId, softReservations);
                        skippedCount++;
                        continue;
                    }

                    // Резервируем ячейки PICKUP (меняем статус)
                    var reservedCells = packingResult.PackedItems.Select(p => p.TargetPositionId).Distinct().ToList();
                    await _db.GetTable<PositionModel>()
                             .Where(p => reservedCells.Contains(p.PositionId))
                             .Set(p => p.Status, "Reserved")
                             .UpdateAsync();

                    _logger.LogInformation("|   > зарезервировано {Count} ячеек PICKUP: [{CellIds}]",
                        reservedCells.Count, string.Join(", ", reservedCells));

                    // Создаем базовую задачу
                    var baseTask = new BaseTaskModel
                    {
                        Title = $"Сборка заказа {order.OrderId}",
                        Description = "Автоматически спланированная сборка заказа",
                        BranchId = order.BranchId,
                        Type = "OrderAssembly",
                        Status = "New",
                        PriorityLevel = 1,
                        CreatedAt = DateTime.UtcNow,
                        Deadline = order.DeliveryDate
                    };
                    var taskId = await _db.InsertWithInt32IdentityAsync(baseTask);

                    _logger.LogInformation("|   > создана базовая задача TaskId={TaskId}", taskId);

                    // Выбираем наименее загруженного работника
                    var selectedWorkers = await _baseTaskService.GetAutoSelectedEmployeesAsync(order.BranchId, 1);
                    if (!selectedWorkers.Any())
                    {
                        _logger.LogWarning("|   ! нет доступных работников в филиале {BranchId}, откат задачи TaskId={TaskId}",
                            order.BranchId, taskId);

                        // Откатываем ячейки PICKUP
                        await _db.GetTable<PositionModel>()
                                 .Where(p => reservedCells.Contains(p.PositionId))
                                 .Set(p => p.Status, "Active")
                                 .UpdateAsync();

                        // Откатываем задачу
                        await _db.GetTable<BaseTaskModel>()
                                 .Where(t => t.TaskId == taskId)
                                 .DeleteAsync();

                        await RollbackHardAllocationAsync(order.OrderId, softReservations);
                        skippedCount++;
                        continue;
                    }

                    var assignedUserId = selectedWorkers.First();
                    _logger.LogInformation("|   > подобран работник UserId={UserId} на задачу TaskId={TaskId}",
                        assignedUserId, taskId);

                    // Назначаем сборку
                    var assignment = new OrderAssemblyAssignmentModel
                    {
                        TaskId = taskId,
                        OrderId = order.OrderId,
                        AssignedToUserId = assignedUserId,
                        BranchId = order.BranchId,
                        Status = 0, // Assigned
                        AssignedAt = DateTime.UtcNow
                    };
                    assignment.Id = await _db.InsertWithInt32IdentityAsync(assignment);

                    // Создаем строки сборки
                    foreach (var packedBlock in packingResult.PackedItems)
                    {
                        // Находим оригинальную информацию о позиции для получения исходной ячейки
                        var originalInfo = orderItems.First(o => o.OrderPositionId == packedBlock.OrderPositionId);

                        var line = new OrderAssemblyLineModel
                        {
                            OrderAssemblyAssignmentId = assignment.Id,
                            ItemPositionId = originalInfo.ItemPositionId,
                            SourcePositionId = originalInfo.SourcePositionId,
                            TargetPositionId = packedBlock.TargetPositionId,
                            Quantity = packedBlock.Quantity,
                            Status = 0 // Pending
                        };
                        await _db.InsertAsync(line);
                    }

                    // Обновляем статус заказа
                    await _db.GetTable<OrderModel>()
                             .Where(o => o.OrderId == order.OrderId)
                             .Set(o => o.Status, "Assembling")
                             .UpdateAsync();

                    _logger.LogInformation("|   * завершено, статус -> 'Assembling', {LineCount} строк сборки", orderItems.Count);
                    plannedCount++;
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