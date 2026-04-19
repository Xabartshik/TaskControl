using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services
{
    public class OrderAssemblyExecutionService : IOrderAssemblyExecutionService
    {
        private readonly IOrderAssemblyAssignmentRepository _assignmentRepo;
        private readonly IOrderAssemblyLineRepository _lineRepo;
        private readonly ILogger<OrderAssemblyExecutionService> _logger;
        private readonly AppSettings _appSettings;
        private readonly ITaskDataConnection _db;

        public OrderAssemblyExecutionService(
            IOrderAssemblyAssignmentRepository assignmentRepo,
            IOrderAssemblyLineRepository lineRepo,
            ILogger<OrderAssemblyExecutionService> logger,
            IOptions<AppSettings> appSettings,
            ITaskDataConnection db)
        {
            _assignmentRepo = assignmentRepo ?? throw new ArgumentNullException(nameof(assignmentRepo));
            _lineRepo = lineRepo ?? throw new ArgumentNullException(nameof(lineRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSettings = appSettings?.Value ?? new AppSettings();
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<List<WorkerAssemblyTaskDto>> GetWorkerAssemblyTasks(int userId)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("|   [Exec] получение задач сборки для работника {UserId}", userId);

            // Получаем основные назначения
            var assignments = await _assignmentRepo.GetByUserIdAsync(userId);
            var activeAssignments = assignments
                .Where(a => a.Status == OrderAssemblyAssignmentStatus.Assigned || a.Status == OrderAssemblyAssignmentStatus.InProgress)
                .ToList();

            if (!activeAssignments.Any())
                return new List<WorkerAssemblyTaskDto>();

            var result = new List<WorkerAssemblyTaskDto>();

            // Используем таблицы из разных модулей (благодаря ProjectReference)
            // Примечание: ITaskDataConnection может работать с любыми таблицами через GetTable<T>
            var baseTasks = _db.ActiveTasks; // Это BaseTaskModel из TaskModule
            var positions = _db.GetTable<TaskControl.InventoryModule.DataAccess.Model.PositionModel>();
            var itemPositions = _db.GetTable<TaskControl.InventoryModule.DataAccess.Model.ItemPositionModel>();
            var items = _db.GetTable<TaskControl.InformationModule.DataAccess.Model.ItemModel>();

            foreach (var a in activeAssignments)
            {
                // Данные из base_tasks
                var taskModel = await baseTasks.FirstOrDefaultAsync(t => t.TaskId == a.TaskId);

                var dto = new WorkerAssemblyTaskDto
                {
                    AssignmentId = a.Id,
                    TaskId = a.TaskId,
                    TaskNumber = taskModel?.Title ?? $"T-{a.TaskId}", // Используем Title как номер задания
                    OrderId = a.OrderId,
                    Status = a.Status,
                    CreatedDate = taskModel?.CreatedAt,
                    TotalLines = a.TotalLines
                };

                // Группировка по ячейкам PICKUP
                var cellGroups = a.Lines.GroupBy(l => l.TargetPositionId);
                foreach (var g in cellGroups)
                {
                    var targetId = g.Key;
                    var posModel = await positions.FirstOrDefaultAsync(p => p.PositionId == targetId);
                    var fullCode = GetFullPositionCode(posModel) ?? targetId.ToString();

                    var cellDto = new CellPlacementInfoDto
                    {
                        TargetPositionId = targetId,
                        CellCode = fullCode,
                        CellDisplayName = fullCode
                    };

                    foreach (var l in g)
                    {
                        // Получаем инфо о товаре
                        var itemInfo = await (from ip in itemPositions
                                              join i in items on ip.ItemId equals i.ItemId
                                              where ip.Id == l.ItemPositionId
                                              select new { i.ItemId, i.Name }).FirstOrDefaultAsync();

                        cellDto.Items.Add(new PlacementLineDto
                        {
                            LineId = l.Id,
                            ItemPositionId = l.ItemPositionId,
                            ItemId = itemInfo?.ItemId ?? 0,
                            ItemName = itemInfo?.Name ?? "Неизвестный товар",
                            Barcode = (itemInfo?.ItemId ?? 0).ToString(), // Fallback штрих-кода
                            Quantity = l.Quantity,
                            PickedQuantity = l.PickedQuantity,
                            Status = l.Status
                        });
                    }
                    dto.CellPlacements.Add(cellDto);
                }
                result.Add(dto);
            }

            return result;
        }

        public async Task ScanAndPickItem(int lineId, string scannedBarcode)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("|   [Exec] взят товар: LineId={LineId}, Barcode={Barcode}", lineId, scannedBarcode);

            var line = await _lineRepo.GetByIdAsync(lineId);
            if (line == null) throw new ArgumentException("Line not found");

            // TODO: Проверка штрих-кода через репозиторий товаров
            line.MarkAsPicked();
            await _lineRepo.UpdateAsync(line);
        }

        public async Task<BulkPlaceResultDto> ScanAndPlaceBulk(int assignmentId, string scannedCellCode)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("|   [Exec] массовое размещение: AssignmentId={AssignmentId}, CellCode={CellCode}", assignmentId, scannedCellCode);

            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (assignment == null) throw new ArgumentException("Assignment not found");

            // Пытаемся найти целевую ячейку по ID (если в QR только число) или по полному коду
            var positions = _db.GetTable<TaskControl.InventoryModule.DataAccess.Model.PositionModel>();
            TaskControl.InventoryModule.DataAccess.Model.PositionModel? targetPosition = null;

            if (int.TryParse(scannedCellCode, out int scannedId))
            {
                // Если отсканировано число, ищем напрямую по ID среди всех ячеек
                targetPosition = await positions.FirstOrDefaultAsync(p => p.PositionId == scannedId);
            }
            
            if (targetPosition == null)
            {
                // Запасной вариант: ищем по полному коду среди ячеек PICKUP
                var allPickupPositions = await positions.Where(p => p.ZoneCode == "PICKUP").ToListAsync();
                targetPosition = allPickupPositions.FirstOrDefault(p => GetFullPositionCode(p) == scannedCellCode);
            }

            if (targetPosition == null)
                throw new ArgumentException($"Ячейка с кодом или ID '{scannedCellCode}' не найдена.");

            var targetPositionId = targetPosition.PositionId;

            // Находим все строки этого задания, которые уже были собраны (Picked)
            // и предназначены для отсканированной ячейки
            var linesToPlace = assignment.Lines
                .Where(l => l.Status == OrderAssemblyLineStatus.Picked && l.TargetPositionId == targetPositionId)
                .ToList();

            if (linesToPlace.Count == 0)
                throw new InvalidOperationException("Нет собранных товаров, предназначенных для этой ячейки.");

            // Переводим все найденные строки в статус Placed
            foreach (var line in linesToPlace)
            {
                line.MarkAsPlaced();
                await _lineRepo.UpdateAsync(line);
            }

            // Считаем оставшиеся ячейки, в которые ещё нужно разложить товары
            var allPickedForOtherCells = assignment.Lines
                .Where(l => l.Status == OrderAssemblyLineStatus.Picked && l.TargetPositionId != targetPositionId)
                .Select(l => l.TargetPositionId)
                .Distinct()
                .Count();

            return new BulkPlaceResultDto
            {
                PlacedCount = linesToPlace.Count,
                RemainingCells = allPickedForOtherCells
            };
        }

        public async Task ReportMissingItem(int lineId, string reason)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("|   ! недостача: LineId={LineId}, Reason={Reason}", lineId, reason);

            var line = await _lineRepo.GetByIdAsync(lineId);
            if (line == null) throw new ArgumentException("Line not found");

            line.ReportDiscrepancy();
            await _lineRepo.UpdateAsync(line);

            // TODO: Добавить запись в inventory_discrepancies
        }

        public async Task CompleteAssemblyTask(int assignmentId)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("|   * завершение задачи сборки AssignmentId={AssignmentId}", assignmentId);

            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (assignment == null) throw new ArgumentException("Assignment not found");

            if (assignment.Lines.Any(l => l.Status == OrderAssemblyLineStatus.Pending || l.Status == OrderAssemblyLineStatus.Picked))
                throw new InvalidOperationException("Not all items are placed or reported as missing.");

            assignment.Complete(DateTime.UtcNow);
            await _assignmentRepo.UpdateAsync(assignment);

            var conn = (DataConnection)_db;
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Снимаем статус Reserved с ячеек PICKUP
                var targetCells = assignment.Lines.Select(l => l.TargetPositionId).Distinct().ToList();
                foreach (var cellId in targetCells)
                {
                    await conn.ExecuteAsync("UPDATE positions SET status = 'Active' WHERE position_id = @id", new DataParameter("id", cellId));
                }

                // Меняем статус заказа на Assembled
                await conn.ExecuteAsync("UPDATE orders SET status = 'Assembled' WHERE order_id = @orderId", new DataParameter("orderId", assignment.OrderId));
                
                // Переводим задачу base_tasks в Completed
                await conn.ExecuteAsync("UPDATE base_tasks SET status = 'Completed', completed_at = @dt WHERE task_id = @taskId", 
                    new DataParameter("dt", DateTime.UtcNow),
                    new DataParameter("taskId", assignment.TaskId)
                );

                // Создаем записи о movement
                foreach (var line in assignment.Lines.Where(l => l.Status == OrderAssemblyLineStatus.Placed))
                {
                    await conn.ExecuteAsync(@"
                        INSERT INTO item_movements (source_item_position_id, destination_position_id, quantity)
                        VALUES (@itemPosId, @destPos, @qty)",
                        new DataParameter("itemPosId", line.ItemPositionId),
                        new DataParameter("destPos", line.TargetPositionId),
                        new DataParameter("qty", line.Quantity));

                    // Обновляем item_positions
                    await conn.ExecuteAsync(@"
                        UPDATE item_positions SET position_id = @targetPos WHERE id = @itemPosId",
                        new DataParameter("itemPosId", line.ItemPositionId),
                        new DataParameter("targetPos", line.TargetPositionId));
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        private string GetFullPositionCode(TaskControl.InventoryModule.DataAccess.Model.PositionModel pos)
        {
            if (pos == null) return null;

            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(pos.ZoneCode)) parts.Add(pos.ZoneCode);
            if (!string.IsNullOrEmpty(pos.FirstLevelStorageType)) parts.Add(pos.FirstLevelStorageType);
            if (!string.IsNullOrEmpty(pos.FLSNumber)) parts.Add(pos.FLSNumber);
            if (!string.IsNullOrEmpty(pos.SecondLevelStorage)) parts.Add(pos.SecondLevelStorage);
            if (!string.IsNullOrEmpty(pos.ThirdLevelStorage)) parts.Add(pos.ThirdLevelStorage);

            return string.Join("-", parts);
        }
    }
}
