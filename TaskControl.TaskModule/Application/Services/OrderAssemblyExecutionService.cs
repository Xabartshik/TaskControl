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
                _logger.LogTrace("Получение задач сборки для работника {UserId}", userId);

            var assignments = await _assignmentRepo.GetByUserIdAsync(userId);
            var activeAssignments = assignments
                .Where(a => a.Status == OrderAssemblyAssignmentStatus.Assigned || a.Status == OrderAssemblyAssignmentStatus.InProgress)
                .Select(a => new WorkerAssemblyTaskDto
                {
                    AssignmentId = a.Id,
                    TaskId = a.TaskId,
                    OrderId = a.OrderId,
                    Status = a.Status,
                    TotalLines = a.TotalLines,
                    // Группируем товары по целевой ячейке, чтобы работник видел,
                    // какие товары нужно положить в каждую ячейку PICKUP
                    CellPlacements = a.Lines
                        .GroupBy(l => l.TargetPositionId)
                        .Select(g => new CellPlacementInfoDto
                        {
                            TargetPositionId = g.Key,
                            Items = g.Select(l => new PlacementLineDto
                            {
                                LineId = l.Id,
                                ItemPositionId = l.ItemPositionId,
                                Quantity = l.Quantity,
                                Status = l.Status
                            }).ToList()
                        })
                        .ToList()
                })
                .ToList();

            return activeAssignments;
        }

        public async Task ScanAndPickItem(int lineId, string scannedBarcode)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("Кладовщик взял товар (LineId: {LineId}, Barcode: {Barcode})", lineId, scannedBarcode);

            var line = await _lineRepo.GetByIdAsync(lineId);
            if (line == null) throw new ArgumentException("Line not found");

            // TODO: Проверка штрих-кода через репозиторий товаров
            line.MarkAsPicked();
            await _lineRepo.UpdateAsync(line);
        }

        public async Task<BulkPlaceResultDto> ScanAndPlaceBulk(int assignmentId, string scannedCellCode)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("Массовое размещение для задания {AssignmentId}, ячейка {CellCode}", assignmentId, scannedCellCode);

            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (assignment == null) throw new ArgumentException("Assignment not found");

            // Находим целевую ячейку по её коду (fls_number)
            var conn = (DataConnection)_db;
            var targetPositionId = await conn.ExecuteAsync<int?>(
                "SELECT position_id FROM positions WHERE fls_number = @code AND zone_code = 'PICKUP' LIMIT 1",
                new DataParameter("code", scannedCellCode));

            if (targetPositionId == null || targetPositionId == 0)
                throw new ArgumentException($"Ячейка PICKUP с кодом '{scannedCellCode}' не найдена.");

            // Находим все строки этого задания, которые уже были собраны (Picked)
            // и предназначены для отсканированной ячейки
            var linesToPlace = assignment.Lines
                .Where(l => l.Status == OrderAssemblyLineStatus.Picked && l.TargetPositionId == targetPositionId.Value)
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
                .Where(l => l.Status == OrderAssemblyLineStatus.Picked && l.TargetPositionId != targetPositionId.Value)
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
                _logger.LogTrace("Репортируем недостачу (LineId: {LineId}, Reason: {Reason})", lineId, reason);

            var line = await _lineRepo.GetByIdAsync(lineId);
            if (line == null) throw new ArgumentException("Line not found");

            line.ReportDiscrepancy();
            await _lineRepo.UpdateAsync(line);

            // TODO: Добавить запись в inventory_discrepancies
        }

        public async Task CompleteAssemblyTask(int assignmentId)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("Завершение задачи сборки {AssignmentId}", assignmentId);

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
    }
}
