using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
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
            {
                _logger.LogTrace("Запуск OrderAssemblyPlannerJob (планирование сборок заказов)");
            }

            try
            {
                var conn = (DataConnection)_db;
                using var transaction = await conn.BeginTransactionAsync();

                var targetTime = DateTime.UtcNow.AddHours(2);

                var targetOrdersSql = @"
                    SELECT order_id, branch_id 
                    FROM orders 
                    WHERE status = 'New' AND delivery_date <= @targetTime";
                
                var targetOrders = await conn.QueryToListAsync<OrderInfo>(targetOrdersSql, new DataParameter("targetTime", targetTime));

                _logger.LogInformation("+--- Планирование сборки заказов: найдено {Count} заказов (до {TargetTime:yyyy-MM-dd HH:mm})",
                    targetOrders.Count, targetTime);

                var plannedCount = 0;
                var skippedCount = 0;

                foreach (var order in targetOrders)
                {
                    _logger.LogInformation("|\n| [Заказ #{OrderId}] филиал {BranchId}", order.order_id, order.branch_id);

                    // Получаем позиции заказа
                    var positionsSql = @"
                        SELECT op.unique_id AS OrderPositionId, ip.item_id AS ItemId, ip.id AS ItemPositionId, 
                               i.length, i.width, i.height, p.position_id as SourcePositionId,
                               ores.quantity AS Quantity
                        FROM order_positions op
                        JOIN order_reservations ores ON op.unique_id = ores.order_position_id
                        JOIN item_positions ip ON ores.item_position_id = ip.id
                        JOIN items i ON ip.item_id = i.item_id
                        JOIN positions p ON ip.position_id = p.position_id
                        WHERE op.order_id = @orderId";

                    var orderItems = await conn.QueryToListAsync<RawItemToPack>(positionsSql, new DataParameter("orderId", order.order_id));

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
                        Length = x.length,
                        Width = x.width,
                        Height = x.height
                    }).ToList();

                    // Ищем свободные ячейки PICKUP в этом же branch_id
                    var pickupCellsSql = @"
                        SELECT p.position_id PositionId, p.length, p.width, p.height
                        FROM positions p
                        LEFT JOIN item_positions ip ON p.position_id = ip.position_id
                        WHERE p.branch_id = @branchId 
                          AND p.zone_code = 'PICKUP' 
                          AND p.status = 'Active'
                          AND ip.position_id IS NULL"; 
                          
                    var availableCells = await conn.QueryToListAsync<CellToPackInto>(pickupCellsSql, new DataParameter("branchId", order.branch_id));

                    // Пакуем
                    var packingResult = _packingService.AssignItemsToPickupCells(itemsToPack, availableCells);
                    
                    if (packingResult.ItemToCellMap.Count < itemsToPack.Count)
                    {
                        _logger.LogWarning("|   ! не удалось распланировать: нужно {Required} ячеек PICKUP, доступно {Available}",
                            itemsToPack.Count, availableCells.Count);
                        skippedCount++;
                        continue;
                    }

                    // Резервируем ячейки PICKUP
                    var reservedCells = packingResult.ItemToCellMap.Values.Distinct().ToList();
                    foreach (var cellId in reservedCells)
                    {
                        await conn.ExecuteAsync("UPDATE positions SET status = 'Reserved' WHERE position_id = @id", new DataParameter("id", cellId));
                    }
                    _logger.LogInformation("|   > зарезервировано {Count} ячеек PICKUP: [{CellIds}]",
                        reservedCells.Count, string.Join(", ", reservedCells));

                    // Создаем базовую задачу
                    var taskId = await conn.ExecuteAsync<int>(@"
                        INSERT INTO base_tasks (title, description, branch_id, type, status, priority)
                        VALUES (@title, @desc, @branch, 'OrderAssembly', 'New', 7) RETURNING task_id;",
                        new DataParameter("title", $"Сборка заказа {order.order_id}"),
                        new DataParameter("desc", "Автоматически спланированная сборка заказа"),
                        new DataParameter("branch", order.branch_id));

                    _logger.LogInformation("|   > создана базовая задача TaskId={TaskId}", taskId);

                    // Выбираем наименее загруженного работника через BaseTaskService
                    var selectedWorkers = await _baseTaskService.GetAutoSelectedEmployeesAsync(order.branch_id, 1);
                    if (!selectedWorkers.Any())
                    {
                        _logger.LogWarning("|   ! нет доступных работников в филиале {BranchId}, откат задачи TaskId={TaskId}",
                            order.branch_id, taskId);
                        // Откатываем резервирование ячеек
                        foreach (var cellId in reservedCells)
                        {
                            await conn.ExecuteAsync("UPDATE positions SET status = 'Active' WHERE position_id = @id", new DataParameter("id", cellId));
                        }
                        // Откатываем вставку base_task
                        await conn.ExecuteAsync("DELETE FROM base_tasks WHERE task_id = @taskId", new DataParameter("taskId", taskId));
                        skippedCount++;
                        continue;
                    }

                    var assignedUserId = selectedWorkers.First();

                    _logger.LogInformation("|   > подобран работник UserId={UserId} на задачу TaskId={TaskId}",
                        assignedUserId, taskId);

                    // Создаем в БД order_assembly_assignments
                    var assignment = new OrderAssemblyAssignmentModel
                    {
                        TaskId = taskId,
                        OrderId = order.order_id,
                        AssignedToUserId = assignedUserId,
                        BranchId = order.branch_id,
                        Status = 0, // Assigned
                        AssignedAt = DateTime.UtcNow
                    };
                    assignment.Id = await _db.InsertWithInt32IdentityAsync(assignment);

                    _logger.LogInformation("|   > создано назначение сборки AssignmentId={AssignmentId}", assignment.Id);

                    foreach (var packItem in orderItems)
                    {
                        var targetCell = packingResult.ItemToCellMap[packItem.OrderPositionId];
                        var line = new OrderAssemblyLineModel
                        {
                            OrderAssemblyAssignmentId = assignment.Id,
                            ItemPositionId = packItem.ItemPositionId,
                            SourcePositionId = packItem.SourcePositionId,
                            TargetPositionId = targetCell,
                            Quantity = packItem.Quantity,
                            Status = 0 // Pending
                        };
                        await _db.InsertAsync(line);

                        _logger.LogDebug("|      - ItemPositionId={ItemPosId}: {SourcePos} -> {TargetPos}",
                            packItem.ItemPositionId, packItem.SourcePositionId, targetCell);
                    }

                    // Обновляем статус заказа
                    await conn.ExecuteAsync("UPDATE orders SET status = 'Assembling' WHERE order_id = @orderId", new DataParameter("orderId", order.order_id));

                    _logger.LogInformation("|   * завершено, статус -> 'Assembling', {LineCount} строк сборки",
                        orderItems.Count);
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

        private class OrderInfo
        {
            public int order_id { get; set; }
            public int branch_id { get; set; }
        }

        private class RawItemToPack
        {
            public int OrderPositionId { get; set; }
            public int ItemId { get; set; }
            public int ItemPositionId { get; set; }
            public double length { get; set; }
            public double width { get; set; }
            public double height { get; set; }
            public int SourcePositionId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
