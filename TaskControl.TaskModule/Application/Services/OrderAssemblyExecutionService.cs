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
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services
{
    public class OrderAssemblyExecutionService : IOrderAssemblyExecutionService
    {
        private readonly IOrderAssemblyAssignmentRepository _assignmentRepo;
        private readonly IOrderAssemblyLineRepository _lineRepo;
        private readonly ILogger<OrderAssemblyExecutionService> _logger;
        private readonly ITelemetryService _telemetryService;
        private readonly TaskWorkloadAggregator _aggregator;
        private readonly AppSettings _appSettings;
        private readonly ITaskDataConnection _db;

        public OrderAssemblyExecutionService(
            IOrderAssemblyAssignmentRepository assignmentRepo,
            IOrderAssemblyLineRepository lineRepo,
            ILogger<OrderAssemblyExecutionService> logger,
            IOptions<AppSettings> appSettings,
            ITelemetryService telemetryService,
            TaskWorkloadAggregator aggregator, 
            ITaskDataConnection db)
        {
            _assignmentRepo = assignmentRepo ?? throw new ArgumentNullException(nameof(assignmentRepo));
            _lineRepo = lineRepo ?? throw new ArgumentNullException(nameof(lineRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSettings = appSettings?.Value ?? new AppSettings();
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
        }

        //public async Task<List<WorkerAssemblyTaskDto>> GetWorkerAssemblyTasks(int userId)
        //{
        //    if (_appSettings.EnableDetailedLogging)
        //        _logger.LogTrace("|   [Exec] получение задач сборки для работника {UserId}", userId);

        //    var assignments = await _assignmentRepo.GetByUserIdAsync(userId);
        //    var activeAssignments = assignments
        //        .Where(a => a.Status == AssignmentStatus.Assigned || a.Status == AssignmentStatus.InProgress)
        //        .ToList();

        //    if (!activeAssignments.Any())
        //        return new List<WorkerAssemblyTaskDto>();

        //    var result = new List<WorkerAssemblyTaskDto>();

        //    var baseTasks = _db.GetTable<BaseTaskModel>();
        //    var positions = _db.GetTable<PositionModel>();
        //    var itemPositions = _db.GetTable<ItemPositionModel>();
        //    var items = _db.GetTable<ItemModel>();

        //    foreach (var a in activeAssignments)
        //    {
        //        var taskModel = await baseTasks.FirstOrDefaultAsync(t => t.TaskId == a.TaskId);

        //        var dto = new WorkerAssemblyTaskDto
        //        {
        //            AssignmentId = a.Id,
        //            TaskId = a.TaskId,
        //            TaskNumber = taskModel?.Title ?? $"T-{a.TaskId}",
        //            OrderId = a.OrderId,
        //            Status = a.Status,
        //            CreatedDate = taskModel?.CreatedAt,
        //            TotalLines = a.TotalLines
        //        };

        //        var cellGroups = a.Lines.GroupBy(l => l.TargetPositionId);
        //        foreach (var g in cellGroups)
        //        {
        //            var targetId = g.Key;
        //            var posModel = await positions.FirstOrDefaultAsync(p => p.PositionId == targetId);
        //            var fullCode = GetFullPositionCode(posModel) ?? targetId.ToString();

        //            var cellDto = new CellPlacementInfoDto
        //            {
        //                TargetPositionId = targetId,
        //                CellCode = fullCode,
        //                CellDisplayName = fullCode
        //            };

        //            foreach (var l in g)
        //            {
        //                var itemInfo = await (from ip in itemPositions
        //                                      join i in items on ip.ItemId equals i.ItemId
        //                                      where ip.Id == l.ItemPositionId
        //                                      select new { i.ItemId, i.Name }).FirstOrDefaultAsync();

        //                cellDto.Items.Add(new PlacementLineDto
        //                {
        //                    LineId = l.Id,
        //                    ItemPositionId = l.ItemPositionId,
        //                    ItemId = itemInfo?.ItemId ?? 0,
        //                    ItemName = itemInfo?.Name ?? "Неизвестный товар",
        //                    Barcode = (itemInfo?.ItemId ?? 0).ToString(),
        //                    Quantity = l.Quantity,
        //                    PickedQuantity = l.PickedQuantity,
        //                    Status = l.Status
        //                });
        //            }
        //            dto.CellPlacements.Add(cellDto);
        //        }
        //        result.Add(dto);
        //    }

        //    return result;
        //}

        public async Task ScanAndPickItem(int lineId, string scannedBarcode)
        {
            var line = await _lineRepo.GetByIdAsync(lineId);
            if (line == null) throw new ArgumentException("Line not found");

            line.MarkAsPicked();
            await _lineRepo.UpdateAsync(line);
        }

        public async Task<BulkPlaceResultDto> ScanAndPlaceBulk(int assignmentId, string scannedCellCode)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (assignment == null) throw new ArgumentException("Assignment not found");

            var positions = _db.GetTable<PositionModel>();
            PositionModel? targetPosition = null;

            if (int.TryParse(scannedCellCode, out int scannedId))
            {
                targetPosition = await positions.FirstOrDefaultAsync(p => p.PositionId == scannedId);
            }

            if (targetPosition == null)
            {
                var allPickupPositions = await positions.Where(p => p.ZoneCode == "PICKUP").ToListAsync();
                targetPosition = allPickupPositions.FirstOrDefault(p => GetFullPositionCode(p) == scannedCellCode);
            }

            if (targetPosition == null)
                throw new ArgumentException($"Ячейка с кодом или ID '{scannedCellCode}' не найдена.");

            var targetPositionId = targetPosition.PositionId;

            var linesToPlace = assignment.Lines
                .Where(l => l.Status == OrderAssemblyLineStatus.Picked && l.TargetPositionId == targetPositionId)
                .ToList();

            if (linesToPlace.Count == 0)
                throw new InvalidOperationException("Нет собранных товаров для этой ячейки.");

            foreach (var line in linesToPlace)
            {
                line.MarkAsPlaced();
                await _lineRepo.UpdateAsync(line);
            }

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
            var line = await _lineRepo.GetByIdAsync(lineId);
            if (line == null) throw new ArgumentException("Line not found");

            line.ReportDiscrepancy();
            await _lineRepo.UpdateAsync(line);
        }

        public async Task CompleteAssemblyTask(int assignmentId)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (assignment == null) throw new ArgumentException("Assignment not found");

            if (assignment.Lines.Any(l => l.Status == OrderAssemblyLineStatus.Pending || l.Status == OrderAssemblyLineStatus.Picked))
                throw new InvalidOperationException("Не все товары размещены.");

            assignment.Complete(DateTime.UtcNow);
            await _assignmentRepo.UpdateAsync(assignment);

            // Начинаем транзакцию через DataConnection
            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();

            try
            {
                // 1. Освобождаем ячейки PICKUP (Status = Active)
                var targetCellIds = assignment.Lines.Select(l => l.TargetPositionId).Distinct().ToList();
                await _db.GetTable<PositionModel>()
                    .Where(p => targetCellIds.Contains(p.PositionId))
                    .Set(p => p.Status, "Active")
                    .UpdateAsync();

                // 2. Обновляем статус заказа
                await _db.GetTable<OrderModel>()
                    .Where(o => o.OrderId == assignment.OrderId)
                    .Set(o => o.Status, "Assembled")
                    .UpdateAsync();

                // 3. Обновляем базовую задачу
                await _db.GetTable<BaseTaskModel>()
                    .Where(t => t.TaskId == assignment.TaskId)
                    .Set(t => t.Status, "Completed")
                    .Set(t => t.CompletedAt, DateTime.UtcNow)
                    .UpdateAsync();

                // 4. Перемещение товаров
                var itemPositions = _db.GetTable<ItemPositionModel>();
                var movements = _db.GetTable<ItemMovementModel>();

                foreach (var line in assignment.Lines.Where(l => l.Status == OrderAssemblyLineStatus.Placed))
                {
                    // Фиксируем передвижение
                    await _db.InsertAsync(new ItemMovementModel
                    {
                        SourceItemPositionId = line.ItemPositionId,
                        DestinationPositionId = line.TargetPositionId,
                        Quantity = line.Quantity,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Уменьшаем остаток на исходной полке
                    await itemPositions
                        .Where(ip => ip.Id == line.ItemPositionId)
                        .Set(ip => ip.Quantity, ip => ip.Quantity - line.Quantity)
                        .UpdateAsync();

                    // Создаем запись в ячейке PICKUP
                    // Находим ID товара из исходной записи
                    var originalItem = await itemPositions.FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId);
                    if (originalItem != null)
                    {
                        await _db.InsertAsync(new ItemPositionModel
                        {
                            ItemId = originalItem.ItemId,
                            PositionId = line.TargetPositionId,
                            Quantity = line.Quantity,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await itemPositions.Where(ip => ip.Quantity <= 0).DeleteAsync();

                // Внедрение отчетов
                int itemsProcessed = assignment.Lines.Sum(line => line.Quantity);
                int branchId = assignment.BranchId;

                DateTime startTime = assignment.StartedAt ?? assignment.AssignedAt;
                int durationSeconds = (int)(DateTime.UtcNow - startTime).TotalSeconds;

                int waitTimeSeconds = assignment.StartedAt.HasValue
                    ? (int)(assignment.StartedAt.Value - assignment.AssignedAt).TotalSeconds
                    : 0;

                int globalQueueSize = await _aggregator.GetTotalActiveWorkloadAsync(assignment.AssignedToUserId);
                await _telemetryService.LogTaskEventAsync(
                    workerId: assignment.AssignedToUserId,
                    branchId: branchId,
                    taskCategory: "OrderAssembly",
                    itemsProcessed: itemsProcessed,
                    durationSeconds: durationSeconds,
                    discrepanciesFound: 0,
                    waitTimeSeconds: waitTimeSeconds,
                    queueSize: globalQueueSize
                );

                await transaction.CommitAsync();

                _logger.LogInformation("|   === Задача сборки TaskId={TaskId} (Assignment {Id}) успешно ЗАВЕРШЕНА ===",
                    assignment.TaskId, assignmentId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "|   !!! Ошибка при завершении задачи {Id}", assignmentId);
                throw;
            }
        }

        public async Task<List<OrderAssemblyHeaderDto>> GetAssignmentsHeaderForWorkerAsync(int userId)
        {
            // 1. Получаем список назначений из репозитория (уже в памяти)
            var assignments = await _assignmentRepo.GetByUserIdAsync(userId);

            // 2. Выбираем только активные, чтобы не грузить лишние задачи
            var activeAssignments = assignments
                .Where(a => a.Status == AssignmentStatus.Assigned || a.Status == AssignmentStatus.InProgress)
                .ToList();

            // 3. Получаем ID всех нужных задач, чтобы выгрузить их одним запросом
            var taskIds = activeAssignments.Select(a => a.TaskId).Distinct().ToList();
    
            // 4. Загружаем задачи из БД (один запрос вместо N)
            var tasks = await _db.GetTable<BaseTaskModel>()
                .Where(t => taskIds.Contains(t.TaskId))
                .ToDictionaryAsync(t => t.TaskId); // Словарь для быстрого поиска по Id

            // 5. Формируем итоговый DTO
            return activeAssignments
                .Select(a => new OrderAssemblyHeaderDto
                {
                    AssignmentId = a.Id,
                    TaskId = a.TaskId,
                    OrderId = a.OrderId,
                    Status = a.Status,
                    Deadline = tasks.TryGetValue(a.TaskId, out var task) ? task.Deadline : null,
                    AssignedAt = a.AssignedAt,
                    TotalLines = a.TotalLines,
                    PlacedLines = a.Lines?.Count(l => l.Status == OrderAssemblyLineStatus.Placed) ?? 0
                })
                .OrderByDescending(a => a.Status == AssignmentStatus.InProgress)
                .ThenBy(a => a.AssignedAt)
                .ToList();
        }


        public async Task<WorkerAssemblyTaskDto> GetAssemblyTaskDetailsAsync(int assignmentId)
        {
            var a = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (a == null) throw new InvalidOperationException("Назначение не найдено.");

            var baseTasks = _db.GetTable<BaseTaskModel>();
            var taskModel = await baseTasks.FirstOrDefaultAsync(t => t.TaskId == a.TaskId);

            var dto = new WorkerAssemblyTaskDto
            {
                AssignmentId = a.Id,
                TaskId = a.TaskId,
                TaskNumber = taskModel?.Title ?? $"T-{a.TaskId}",
                OrderId = a.OrderId,
                Status = a.Status,
                Deadline = taskModel?.Deadline,
                CreatedDate = taskModel?.CreatedAt,
                TotalLines = a.TotalLines
            };

            var positions = _db.GetTable<PositionModel>();
            var itemPositions = _db.GetTable<ItemPositionModel>();
            var items = _db.GetTable<ItemModel>();

            // Группировка по целевой ячейке (куда нужно положить)
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
                    // Добавлено извлечение ip.PositionId (ID исходной ячейки хранения)
                    var itemInfo = await (from ip in itemPositions
                                          join i in items on ip.ItemId equals i.ItemId
                                          where ip.Id == l.ItemPositionId
                                          select new { i.ItemId, i.Name, ip.PositionId }).FirstOrDefaultAsync();

                    // Определяем строковый код исходной ячейки
                    string sourceCellCode = "Неизвестная ячейка";
                    if (itemInfo != null)
                    {
                        var sourcePosModel = await positions.FirstOrDefaultAsync(p => p.PositionId == itemInfo.PositionId);
                        sourceCellCode = GetFullPositionCode(sourcePosModel) ?? itemInfo.PositionId.ToString();
                    }

                    cellDto.Items.Add(new PlacementLineDto
                    {
                        LineId = l.Id,
                        ItemPositionId = l.ItemPositionId,
                        ItemId = itemInfo?.ItemId ?? 0,
                        ItemName = itemInfo?.Name ?? "Неизвестный товар",
                        Barcode = (itemInfo?.ItemId ?? 0).ToString(),
                        SourceCellCode = sourceCellCode,
                        Quantity = l.Quantity,
                        PickedQuantity = l.PickedQuantity,
                        Status = l.Status
                    });
                }
                dto.CellPlacements.Add(cellDto);
            }
            return dto;
        }
        public async Task<bool> StartAssemblyAsync(int id)
        {
            var a = await _assignmentRepo.GetByIdAsync(id);
            if (a == null) return false;
            a.Start(DateTime.UtcNow); // Доменный метод базового класса
            await _assignmentRepo.UpdateAsync(a);
            return true;
        }

        public async Task<bool> PauseAssemblyAsync(int id)
        {
            var a = await _assignmentRepo.GetByIdAsync(id);
            if (a == null) return false;
            a.Pause(); // Доменный метод базового класса
            await _assignmentRepo.UpdateAsync(a);
            return true;
        }

        public async Task<bool> CancelAssemblyAsync(int id)
        {
            var a = await _assignmentRepo.GetByIdAsync(id);
            if (a == null) return false;
            a.Cancel(); // Доменный метод базового класса
            await _assignmentRepo.UpdateAsync(a);
            return true;
        }

        private string GetFullPositionCode(PositionModel pos)
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