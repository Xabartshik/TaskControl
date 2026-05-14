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
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.OrderModule.Domain;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services
{
    public class OrderAssemblyExecutionService : IOrderAssemblyExecutionService
    {
        private readonly IOrderCancellationService _cancellationService;
        private readonly IOrderAssemblyAssignmentRepository _assignmentRepo;
        private readonly IOrderAssemblyLineRepository _lineRepo;
        private readonly ILogger<OrderAssemblyExecutionService> _logger;
        private readonly ITelemetryService _telemetryService;
        private readonly TaskWorkloadAggregator _aggregator;
        private readonly AppSettings _appSettings;
        private readonly ITaskDataConnection _db;
        private readonly IQRTokenService _qrTokenService;
        private readonly ITaskComplexityCalculator _complexityCalculator;
        private readonly ReturnTaskGeneratorService _returnTaskGenerator;

        public OrderAssemblyExecutionService(
            IOrderAssemblyAssignmentRepository assignmentRepo,
            IOrderAssemblyLineRepository lineRepo,
            IQRTokenService qRTokenService,
            ILogger<OrderAssemblyExecutionService> logger,
            IOptions<AppSettings> appSettings,
            ITelemetryService telemetryService,
            TaskWorkloadAggregator aggregator, 
            ITaskDataConnection db,
            ITaskComplexityCalculator complexityCalculator,
            ReturnTaskGeneratorService returnTaskGenerator,
            IOrderCancellationService cancellationService)
        {
            _assignmentRepo = assignmentRepo ?? throw new ArgumentNullException(nameof(assignmentRepo));
            _lineRepo = lineRepo ?? throw new ArgumentNullException(nameof(lineRepo));
            _qrTokenService = qRTokenService ?? throw new ArgumentNullException(nameof(qRTokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSettings = appSettings?.Value ?? new AppSettings();
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
            _complexityCalculator = complexityCalculator ?? throw new ArgumentNullException(nameof(complexityCalculator));
            _returnTaskGenerator = returnTaskGenerator ?? throw new ArgumentNullException(nameof(returnTaskGenerator));
            _cancellationService = cancellationService;
        }



        public async Task<(bool Success, string Message)> VerifyHandoverTokenAsync(int assignmentId, string qrToken)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (assignment == null) return (false, "Назначение сборки не найдено");

            var order = await _db.GetTable<OrderModel>().FirstOrDefaultAsync(o => o.OrderId == assignment.OrderId);
            if (order == null) return (false, "Заказ не найден");

            if (order.DeliveryType != DeliveryType.Express.ToString())
                return (false, "Этот заказ не предназначен для экспресс-выдачи.");

            // Валидация самого токена через QRTokenService
            if (!_qrTokenService.ValidateOrderPickupToken(qrToken, out int tokenCustomerId, out int tokenOrderId, out string errorMessage))
            {
                return (false, $"Ошибка QR-кода: {errorMessage}");
            }

            // Сверка токена с текущим заказом
            if (order.CustomerId != tokenCustomerId || order.OrderId != tokenOrderId)
            {
                return (false, "QR-код не принадлежит этому заказу!");
            }

            return (true, "QR-код подтвержден.");
        }

        public async Task<(bool Success, string Message)> HandoverExpressOrder(int assignmentId, string qrToken, Dictionary<int, int>? cancelledLines = null)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (assignment == null) return (false, "Назначение сборки не найдено");

            var order = await _db.GetTable<OrderModel>().FirstOrDefaultAsync(o => o.OrderId == assignment.OrderId);
            if (order == null) return (false, "Заказ не найден");

            if (order.DeliveryType != "Express")
                return (false, "Этот метод предназначен только для Экспресс-выдачи.");

            if (!_qrTokenService.ValidateOrderPickupToken(qrToken, out int tokenCustomerId, out int tokenOrderId, out string errorMessage))
                return (false, $"Ошибка QR-кода: {errorMessage}");

            if (order.CustomerId != tokenCustomerId || order.OrderId != tokenOrderId)
                return (false, "QR-код не принадлежит этому заказу!");

            // === ИСПРАВЛЕНИЕ 1: Грузим линии напрямую из БД, обходя ленивую загрузку ===
            var linesToPlace = await _db.GetTable<OrderAssemblyLineModel>()
                .Where(l => l.OrderAssemblyAssignmentId == assignmentId && l.Status == (int)OrderAssemblyLineStatus.Picked)
                .ToListAsync();

            var itemsToReturn = new List<(int ItemPositionId, int Qty)>();
            bool isFullCancellation = false;
            var positionsToCancel = new Dictionary<int, int>();

            if (cancelledLines != null && cancelledLines.Any())
            {
                int totalPicked = linesToPlace.Sum(l => l.Quantity);
                int totalCancelled = 0;

                foreach (var kvp in cancelledLines)
                {
                    var line = linesToPlace.FirstOrDefault(l => l.Id == kvp.Key);
                    if (line != null)
                    {
                        int qty = Math.Min(kvp.Value, line.Quantity);
                        totalCancelled += qty;
                        positionsToCancel[line.ItemPositionId] = qty;
                    }
                }
                isFullCancellation = (totalPicked > 0 && totalPicked == totalCancelled);
            }

            if (positionsToCancel.Any())
            {
                itemsToReturn = await _cancellationService.ProcessCancellationAsync(order.OrderId, positionsToCancel, isFullCancellation);
            }

            // === ИСПРАВЛЕНИЕ 2: Находим служебную ячейку Express для фиксации в истории ===
            var expressCell = await _db.GetTable<PositionModel>()
                .FirstOrDefaultAsync(p => p.BranchId == assignment.BranchId && p.ZoneCode == "EXPRESS");

            foreach (var line in linesToPlace)
            {
                if (!isFullCancellation || !cancelledLines.ContainsKey(line.Id))
                {
                    line.Status = (int)OrderAssemblyLineStatus.Placed;

                    // Привязываем служебную ячейку
                    if (expressCell != null) line.TargetPositionId = expressCell.PositionId;

                    await _lineRepo.UpdateAsync(line.ToDomain());
                }
            }

            await CompleteAssemblyTask(assignmentId);

            if (!isFullCancellation)
            {
                await _db.GetTable<OrderModel>().Where(o => o.OrderId == order.OrderId)
                    .Set(o => o.Status, "Completed").UpdateAsync();
            }

            if (itemsToReturn.Any())
            {
                await _returnTaskGenerator.GenerateReturnTaskFromCancelledItemsAsync(order.OrderId, assignment.BranchId, itemsToReturn);
                _logger.LogInformation("|   [Express] Создана задача на возврат {Count} позиций для экспресс-заказа #{OrderId}", itemsToReturn.Count, order.OrderId);

                return (true, isFullCancellation
                    ? "Заказ полностью отменен. Создана задача на возврат."
                    : $"Заказ выдан частично. Создана задача на возврат {itemsToReturn.Count} товаров.");
            }

            _logger.LogInformation("|   [Express] Экспресс-заказ #{OrderId} выдан клиенту полностью", order.OrderId);
            return (true, "Экспресс-заказ успешно выдан полностью.");
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

            // --- НОВАЯ ВАЛИДАЦИЯ ШТРИХ-КОДА ---
            // Находим складскую позицию, из которой мы берем товар
            var itemPos = await _db.GetTable<ItemPositionModel>()
                .FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId);

            if (itemPos != null)
            {
                // Находим информацию о самом товаре, чтобы проверить его штрих-код
                var item = await _db.GetTable<ItemModel>()
                    .FirstOrDefaultAsync(i => i.ItemId == itemPos.ItemId);

                // Сравниваем отсканированную строку с реальным штрих-кодом в БД
                if (item == null || item.Barcode != scannedBarcode.Trim())
                {
                    throw new ArgumentException($"Отсканированный штрих-код '{scannedBarcode}' не совпадает с ожидаемым товаром.");
                }
            }
            // -----------------------------------

            line.MarkAsPicked();
            await _lineRepo.UpdateAsync(line);
        }

        public async Task ApplyItemMovementsForCompletedTaskAsync(int taskId)
        {
            var conn = (DataConnection)_db;
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // 1. Получаем базовую задачу и назначения (нужно для логов и телеметрии)
                var baseTask = await _db.GetTable<BaseTaskModel>().FirstOrDefaultAsync(t => t.TaskId == taskId);

                var assignments = await _db.GetTable<OrderAssemblyAssignmentModel>()
                    .Where(a => a.TaskId == taskId).ToListAsync();

                // Находим главного инициатора сборки (или первого попавшегося, если одиночная задача)
                var mainAssignment = assignments.FirstOrDefault(a => a.Role == (int)AssignmentRole.Main) ?? assignments.FirstOrDefault();
                if (mainAssignment == null) return;

                // 2. Получаем все размещенные (Placed) строки
                var assignmentIds = assignments.Select(a => a.Id).ToList();
                var completedLines = await _db.GetTable<OrderAssemblyLineModel>()
                    .Where(l => assignmentIds.Contains(l.OrderAssemblyAssignmentId) && l.Status == 2)
                    .ToListAsync();

                var orderIdsToUpdate = new HashSet<int>();
                var targetCellIds = new HashSet<int>(); // Для освобождения ячеек PICKUP

                foreach (var line in completedLines)
                {
                    var sourceItemPos = await _db.GetTable<ItemPositionModel>()
                        .FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId);
                    if (sourceItemPos == null) continue;

                    var reservation = await _db.GetTable<OrderReservationModel>()
                        .FirstOrDefaultAsync(r => r.ItemPositionId == line.ItemPositionId);

                    if (reservation != null)
                    {
                        var orderPosition = await _db.GetTable<OrderPositionModel>()
                            .FirstOrDefaultAsync(op => op.UniqueId == reservation.OrderPositionId);
                        if (orderPosition != null) orderIdsToUpdate.Add(orderPosition.OrderId);
                    }

                    // --- МАГИЯ 1: Пишем лог перемещения ---
                    await _db.InsertAsync(new ItemMovementModel
                    {
                        ItemId = sourceItemPos.ItemId,
                        SourcePositionId = sourceItemPos.PositionId,
                        DestinationPositionId = line.TargetPositionId, // null для экспресса
                        Quantity = line.Quantity,
                        TaskId = taskId,
                        WorkerId = mainAssignment.AssignedToUserId,
                        CreatedAt = DateTime.UtcNow
                    });

                    int? newTargetItemPosId = null;

                    // 1. Ищем или создаем ЦЕЛЕВУЮ ячейку (если не экспресс)
                    if (line.TargetPositionId.HasValue)
                    {
                        targetCellIds.Add(line.TargetPositionId.Value);

                        var targetItemPos = await _db.GetTable<ItemPositionModel>()
                            .FirstOrDefaultAsync(ip => ip.PositionId == line.TargetPositionId.Value && ip.ItemId == sourceItemPos.ItemId);

                        if (targetItemPos != null)
                        {
                            targetItemPos.Quantity += line.Quantity;
                            await _db.UpdateAsync(targetItemPos);
                            newTargetItemPosId = targetItemPos.Id;
                        }
                        else
                        {
                            newTargetItemPosId = await _db.InsertWithInt32IdentityAsync(new ItemPositionModel
                            {
                                PositionId = line.TargetPositionId.Value,
                                ItemId = sourceItemPos.ItemId,
                                Quantity = line.Quantity,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    // 2. ПЕРЕПРИВЯЗЫВАЕМ РЕЗЕРВ 
                    // (Резерв можно переносить, он "путешествует" вместе с физическим товаром)
                    if (reservation != null)
                    {
                        reservation.ItemPositionId = newTargetItemPosId;
                        await _db.UpdateAsync(reservation);
                    }

                    // 3. ИСПРАВЛЕНИЕ: ТОЛЬКО СПИСЫВАЕМ ОСТАТОК (БЕЗ УДАЛЕНИЯ!)
                    // Ячейка должна остаться в БД (даже с Quantity = 0), 
                    // так как на нее ссылается историческая таблица OrderAssemblyLineModel
                    sourceItemPos.Quantity -= line.Quantity;
                    await _db.UpdateAsync(sourceItemPos);
                }

                // ВАЖНО: Ниже цикла ВЫРЕЗАНА строка: 
                // await _db.GetTable<ItemPositionModel>().Where(ip => ip.Quantity <= 0).DeleteAsync();
                // Глобальное удаление пустых ячеек здесь делать нельзя из-за исторических Foreign Key!

                // --- МАГИЯ 2: Освобождаем ячейки PICKUP (снимаем блокировку) ---
                if (targetCellIds.Any())
                {
                    await _db.GetTable<PositionModel>()
                        .Where(p => targetCellIds.Contains(p.PositionId))
                        .Set(p => p.Status, "Active")
                        .UpdateAsync();
                }

                // --- МАГИЯ 3: Обновляем статусы заказов (с учетом Express) ---
                foreach (var orderId in orderIdsToUpdate)
                {
                    var order = await _db.GetTable<OrderModel>().FirstOrDefaultAsync(o => o.OrderId == orderId);
                    if (order != null && order.Status != OrderStatus.Completed.ToString())
                    {
                        if (order.DeliveryType != DeliveryType.Express.ToString())
                        {
                            order.Status = OrderStatus.Ready.ToString();
                            await _db.UpdateAsync(order);
                        }
                        else
                        {
                            order.Status = OrderStatus.Completed.ToString();
                            await _db.UpdateAsync(order);
                        }
                    }
                }

                // --- МАГИЯ 4: Обновляем статус базовой задачи ---
                if (baseTask != null)
                {
                    baseTask.Status = "Completed";
                    baseTask.CompletedAt = DateTime.UtcNow;
                    await _db.UpdateAsync(baseTask);
                }

                // --- МАГИЯ 5: Пишем телеметрию (аналитика работы) ---
                int itemsProcessed = completedLines.Sum(l => l.Quantity);
                DateTime startTime = mainAssignment.StartedAt ?? mainAssignment.AssignedAt;
                int durationSeconds = (int)(DateTime.UtcNow - startTime).TotalSeconds;
                int waitTimeSeconds = mainAssignment.StartedAt.HasValue
                    ? (int)(mainAssignment.StartedAt.Value - mainAssignment.AssignedAt).TotalSeconds
                    : 0;

                int workerId = mainAssignment.AssignedToUserId ?? 0;

                int globalQueueSize = await _aggregator.GetTotalActiveWorkloadAsync(workerId);

                await _telemetryService.LogTaskEventAsync(
                    workerId: workerId,
                    branchId: baseTask?.BranchId ?? 0,
                    taskCategory: "OrderAssembly",
                    itemsProcessed: itemsProcessed,
                    durationSeconds: durationSeconds,
                    discrepanciesFound: 0,
                    waitTimeSeconds: waitTimeSeconds,
                    queueSize: globalQueueSize
                );

                await transaction.CommitAsync();
                _logger.LogInformation("|   === Задача сборки TaskId={TaskId} успешно ЗАВЕРШЕНА и пост-обработана ===", taskId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "|   !!! Ошибка при пост-обработке задачи сборки {TaskId}", taskId);
                throw;
            }
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
            if (assignment == null) return;

            var order = await _db.GetTable<OrderModel>().FirstOrDefaultAsync(o => o.OrderId == assignment.OrderId);
            var itemPositions = _db.GetTable<ItemPositionModel>();

            // Прямой запрос линий (избегаем пустых списков из-за ленивой загрузки)
            var placedLines = await _db.GetTable<OrderAssemblyLineModel>()
                .Where(l => l.OrderAssemblyAssignmentId == assignmentId && l.Status == (int)OrderAssemblyLineStatus.Placed)
                .ToListAsync();

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                foreach (var line in placedLines)
                {
                    var originalItem = await itemPositions.FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId);

                    if (originalItem != null && originalItem.Quantity > 0)
                    {
                        int qtyToDeduct = Math.Min(originalItem.Quantity, line.Quantity);

                        // Фиксируем перемещение в историю (здесь будет ID ячейки EXPRESS)
                        await _db.InsertAsync(new ItemMovementModel
                        {
                            ItemId = originalItem.ItemId,
                            SourcePositionId = originalItem.PositionId,
                            DestinationPositionId = line.TargetPositionId,
                            Quantity = qtyToDeduct,
                            TaskId = assignment.TaskId,
                            WorkerId = assignment.AssignedToUserId,
                            CreatedAt = DateTime.UtcNow
                        });

                        // Списываем товар с полки хранения
                        await itemPositions
                            .Where(ip => ip.Id == line.ItemPositionId)
                            .Set(ip => ip.Quantity, originalItem.Quantity - qtyToDeduct)
                            .UpdateAsync();

                        // Проверяем, не является ли целевая ячейка зоной EXPRESS
                        bool isExpressZone = false;
                        if (line.TargetPositionId.HasValue)
                        {
                            var targetCell = await _db.GetTable<PositionModel>()
                                .FirstOrDefaultAsync(p => p.PositionId == line.TargetPositionId.Value);
                            isExpressZone = targetCell?.ZoneCode == "EXPRESS";
                        }

                        if (line.TargetPositionId.HasValue && !isExpressZone)
                        {
                            // Для обычной выдачи (PICKUP) создаем остаток в ячейке ожидания
                            var newPosId = await _db.InsertWithInt32IdentityAsync(new ItemPositionModel
                            {
                                ItemId = originalItem.ItemId,
                                PositionId = line.TargetPositionId.Value,
                                Quantity = qtyToDeduct,
                                CreatedAt = DateTime.UtcNow
                            });

                            await _db.GetTable<OrderReservationModel>()
                                .Where(r => r.ItemPositionId == line.ItemPositionId)
                                .Set(r => r.ItemPositionId, newPosId)
                                .UpdateAsync();
                        }
                        else
                        {
                            // Для EXPRESS (или если ячейка не задана) товар уходит в "никуда" (клиенту)
                            // Просто отвязываем резерв, чтобы он не мешал удалению пустых позиций
                            await _db.GetTable<OrderReservationModel>()
                                .Where(r => r.ItemPositionId == line.ItemPositionId)
                                .Set(r => r.ItemPositionId, (int?)null)
                                .UpdateAsync();
                        }
                    }
                }

                // Завершаем назначение сборки
                assignment.Status = AssignmentStatus.Completed;
                assignment.CompletedAt = DateTime.UtcNow;
                await _assignmentRepo.UpdateAsync(assignment);

                await _db.GetTable<BaseTaskModel>()
                    .Where(t => t.TaskId == assignment.TaskId)
                    .Set(t => t.Status, "Completed")
                    .UpdateAsync();

                // Логика создания задачи выдачи (Handover)
                if (order != null && order.DeliveryType != "Express")
                {
                    // Создаем базовую задачу (здесь BranchId нужен)
                    var handoverTaskId = Convert.ToInt32(await _db.InsertWithIdentityAsync(new BaseTaskModel
                    {
                        Title = $"Выдача заказа #{order.OrderId}",
                        Type = "OrderHandover",
                        Status = order.DeliveryType == "Express" ? "Completed" : "New",
                        PriorityLevel = 1,
                        BranchId = assignment.BranchId,
                        CreatedAt = DateTime.UtcNow
                    }));

                    // Создаем назначение (BranchId УДАЛЕН согласно вашей модели)
                    await _db.InsertAsync(new OrderHandoverAssignmentModel
                    {
                        TaskId = handoverTaskId,
                        OrderId = order.OrderId,
                        AssignedToUserId = assignment.AssignedToUserId,
                        Status = order.DeliveryType == "Express" ? (int)AssignmentStatus.Completed : (int)AssignmentStatus.Assigned,
                        Role = "Main",
                        Complexity = 1.0,
                        AssignedAt = DateTime.UtcNow,
                        StartedAt = order.DeliveryType == "Express" ? DateTime.UtcNow : null,
                        CompletedAt = order.DeliveryType == "Express" ? DateTime.UtcNow : null
                    });
                }

                await transaction.CommitAsync();
                _logger.LogInformation("|   === Задача сборки TaskId={TaskId} успешно ЗАВЕРШЕНА ===", assignment.TaskId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при завершении сборки {TaskId}", assignment.TaskId);
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
            // ВАЖНО: Используем _assignmentRepo, который возвращает доменную модель (содержащую список Lines), а не плоскую модель БД
            var a = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (a == null) throw new InvalidOperationException("Назначение не найдено.");

            var baseTasks = _db.GetTable<BaseTaskModel>();
            var taskModel = await baseTasks.FirstOrDefaultAsync(t => t.TaskId == a.TaskId);
            var allTaskAssignments = await _db.GetTable<OrderAssemblyAssignmentModel>()
                                              .Where(x => x.TaskId == a.TaskId)
                                              .ToListAsync();
            var partnerAssignment = allTaskAssignments.FirstOrDefault(x => x.Id != assignmentId);
            bool isCooperative = partnerAssignment != null;
            string partnerName = null;

            if (isCooperative)
            {
                var partnerUser = await _db.GetTable<EmployeeModel>().FirstOrDefaultAsync(u => u.EmployeesId == partnerAssignment.AssignedToUserId);
                partnerName = partnerUser?.FullName ?? $"ID: {partnerAssignment.AssignedToUserId}";
            }

            var dto = new WorkerAssemblyTaskDto
            {
                AssignmentId = a.Id,
                TaskId = a.TaskId,
                TaskNumber = taskModel?.Title ?? $"T-{a.TaskId}",
                OrderId = a.OrderId,
                Status = a.Status,
                Deadline = taskModel?.Deadline,
                CreatedDate = taskModel?.CreatedAt,
                TotalLines = a.TotalLines,
                IsCooperative = isCooperative,
                PartnerName = partnerName,
                PartnerStatus = (AssignmentStatus?)partnerAssignment?.Status
            };

            var positions = _db.GetTable<PositionModel>();
            var itemPositions = _db.GetTable<ItemPositionModel>();
            var items = _db.GetTable<ItemModel>();

            // Группировка по целевой ячейке (targetId теперь int?)
            var cellGroups = a.Lines.GroupBy(l => l.TargetPositionId);
            foreach (var g in cellGroups)
            {
                int? targetId = g.Key;
                string fullCode;

                if (targetId.HasValue)
                {
                    var posModel = await positions.FirstOrDefaultAsync(p => p.PositionId == targetId.Value);
                    fullCode = GetFullPositionCode(posModel) ?? targetId.Value.ToString();
                }
                else
                {
                    // Обработка NULL для экспресс-заказов
                    fullCode = "Экспресс-выдача (в руки)";
                }

                var cellDto = new CellPlacementInfoDto
                {
                    // Если в DTO поле TargetPositionId осталось типа int, 
                    // используем 0 как "виртуальный" ID для экспресс-выдачи
                    TargetPositionId = targetId ?? 0,
                    CellCode = fullCode,
                    CellDisplayName = fullCode
                };

                foreach (var l in g)
                {
                    var itemInfo = await (from ip in itemPositions
                                          join i in items on ip.ItemId equals i.ItemId
                                          where ip.Id == l.ItemPositionId
                                          select new { i.ItemId, i.Name, ip.PositionId }).FirstOrDefaultAsync();

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
        public async Task<bool> StartAssemblyAsync(int assignmentId)
        {
            // 1. Получаем текущее назначение через репозиторий
            var currentAssignment = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (currentAssignment == null) return false;

            // 2. Получаем ВСЕ назначения для этой задачи напрямую из таблицы моделей.
            // ВАЖНО: Используем .ToListAsync(), чтобы получить List в памяти.
            // Это решает ошибку с "Count" (у List есть свойство Count) 
            // и ошибку с определением типов в .First() / .FirstOrDefault()
            var allAssignments = await _db.GetTable<OrderAssemblyAssignmentModel>()
                                          .Where(a => a.TaskId == currentAssignment.TaskId)
                                          .ToListAsync();

            // 3. Если задача одиночная (всего одно назначение в списке)
            if (allAssignments.Count == 1)
            {
                currentAssignment.Start(DateTime.UtcNow);
                await _assignmentRepo.UpdateAsync(currentAssignment);

                // Переводим саму базовую задачу в статус "InProgress"
                await _db.GetTable<BaseTaskModel>()
                         .Where(t => t.TaskId == currentAssignment.TaskId)
                         .Set(t => t.Status, "InProgress")
                         .UpdateAsync();

                return true;
            }

            // 4. КООПЕРАТИВНАЯ ЗАДАЧА:
            // Отмечаем текущего сотрудника как начавшего работу
            currentAssignment.Status = AssignmentStatus.InProgress;
            currentAssignment.StartedAt = DateTime.UtcNow;
            await _assignmentRepo.UpdateAsync(currentAssignment);

            // Ищем напарника среди загруженного списка (уже в памяти)
            // Явно указываем условие поиска
            var partner = allAssignments.FirstOrDefault(a => a.Id != assignmentId);

            // Если напарник найден и он уже нажал "Начать" (статус InProgress)
            if (partner != null && partner.Status == (int)AssignmentStatus.InProgress)
            {
                // Только когда ОБА в процессе, переводим саму задачу BaseTask в статус "InProgress"
                await _db.GetTable<BaseTaskModel>()
                         .Where(t => t.TaskId == currentAssignment.TaskId)
                         .Set(t => t.Status, "InProgress")
                         .UpdateAsync();

                return true;
            }

            // Если напарник еще не нажал кнопку, возвращаем false (мобилка покажет ожидание)
            return false;
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