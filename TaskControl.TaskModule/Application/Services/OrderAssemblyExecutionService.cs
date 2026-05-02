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
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;
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
        private readonly IQRTokenService _qrTokenService;

        public OrderAssemblyExecutionService(
            IOrderAssemblyAssignmentRepository assignmentRepo,
            IOrderAssemblyLineRepository lineRepo,
            IQRTokenService qRTokenService,
            ILogger<OrderAssemblyExecutionService> logger,
            IOptions<AppSettings> appSettings,
            ITelemetryService telemetryService,
            TaskWorkloadAggregator aggregator, 
            ITaskDataConnection db)
        {
            _assignmentRepo = assignmentRepo ?? throw new ArgumentNullException(nameof(assignmentRepo));
            _lineRepo = lineRepo ?? throw new ArgumentNullException(nameof(lineRepo));
            _qrTokenService = qRTokenService ?? throw new ArgumentNullException(nameof(qRTokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSettings = appSettings?.Value ?? new AppSettings();
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
        }

        public async Task<bool> HandoverExpressOrderAsync(int assignmentId, string qrToken)
        {
            // 1. Получаем назначение и заказ
            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (assignment == null) throw new ArgumentException("Назначение сборки не найдено");

            var order = await _db.GetTable<OrderModel>().FirstOrDefaultAsync(o => o.OrderId == assignment.OrderId);
            if (order == null) throw new ArgumentException("Заказ не найден");

            if (order.DeliveryType != DeliveryType.Express.ToString())
                throw new InvalidOperationException("Этот метод предназначен только для Экспресс-выдачи.");

            // 2. Валидация QR-кода покупателя
            if (!_qrTokenService.ValidateOrderPickupToken(qrToken, out int tokenCustomerId, out int tokenOrderId, out string errorMessage))
            {
                throw new ArgumentException($"Ошибка QR-кода: {errorMessage}");
            }

            // Защита: проверяем, что QR принадлежит именно этому клиенту и этому заказу
            if (order.CustomerId != tokenCustomerId || order.OrderId != tokenOrderId)
            {
                throw new ArgumentException("QR-код не принадлежит этому заказу!");
            }

            // 3. Переводим все собранные (Picked) товары в статус размещенных (Placed)
            // Концептуально - мы "размещаем" их прямо в руки клиенту (в зону EXPRESS)
            var linesToPlace = assignment.Lines.Where(l => l.Status == OrderAssemblyLineStatus.Picked).ToList();
            foreach (var line in linesToPlace)
            {
                line.MarkAsPlaced();
                await _lineRepo.UpdateAsync(line);
            }

            // 4. Штатно завершаем задачу сборки (это спишет товары со склада и переместит в виртуальную ячейку выдачи)
            await CompleteAssemblyTask(assignmentId);

            // 5. Переводим заказ сразу в Completed (т.к. клиент уже забрал товар)
            // В CompleteAssemblyTask он переводится в Assembled/Ready, поэтому мы делаем финальный апдейт
            await _db.GetTable<OrderModel>()
                .Where(o => o.OrderId == order.OrderId)
                .Set(o => o.Status, OrderStatus.Completed.ToString()) // Ставим финальный статус
                .UpdateAsync();

            _logger.LogInformation("|   [Express] Экспресс-заказ #{OrderId} успешно выдан клиенту {CustomerId} напрямую со сборки",
                order.OrderId, order.CustomerId);

            return true;
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

        public async Task ApplyItemMovementsForCompletedTaskAsync(int taskId)
        {
            var conn = (DataConnection)_db;
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // 1. Получаем все строки сборки напрямую через JOIN таблиц (обходим отсутствие свойства Lines)
                var completedLinesQuery = from a in _db.GetTable<OrderAssemblyAssignmentModel>()
                                          join l in _db.GetTable<OrderAssemblyLineModel>() on a.Id equals l.OrderAssemblyAssignmentId
                                          where a.TaskId == taskId && l.Status == 2 // 2 = Placed
                                          select l;

                var completedLines = await completedLinesQuery.ToListAsync();
                var orderIdsToUpdate = new HashSet<int>();

                foreach (var line in completedLines)
                {
                    // 2. Достаем исходную позицию
                    var sourceItemPos = await _db.GetTable<ItemPositionModel>()
                        .FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId);

                    if (sourceItemPos == null) continue;

                    // 3. Находим резерв и через него OrderId
                    var reservation = await _db.GetTable<OrderReservationModel>()
                        .FirstOrDefaultAsync(r => r.ItemPositionId == line.ItemPositionId);

                    if (reservation != null)
                    {
                        var orderPosition = await _db.GetTable<OrderPositionModel>()
                            .FirstOrDefaultAsync(op => op.UniqueId == reservation.OrderPositionId);

                        if (orderPosition != null)
                        {
                            orderIdsToUpdate.Add(orderPosition.OrderId);
                        }
                    }

                    // Вычитаем количество (списываем со склада)
                    sourceItemPos.Quantity -= line.Quantity;
                    await _db.UpdateAsync(sourceItemPos);

                    int? newTargetItemPosId = null;

                    // 4. Если есть целевая ячейка (не экспресс-выдача)
                    if (line.TargetPositionId.HasValue)
                    {
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
                            var newItemPos = new ItemPositionModel
                            {
                                PositionId = line.TargetPositionId.Value, // Явное извлечение значения (.Value)
                                ItemId = sourceItemPos.ItemId,
                                Quantity = line.Quantity,
                                CreatedAt = DateTime.UtcNow
                            };
                            newTargetItemPosId = await _db.InsertWithInt32IdentityAsync(newItemPos);
                        }
                    }

                    // 5. Обновляем резерв: либо привязываем к новой ячейке (Pickup), либо отвязываем (NULL) для экспресса
                    if (reservation != null)
                    {
                        reservation.ItemPositionId = newTargetItemPosId;
                        await _db.UpdateAsync(reservation);
                    }
                }

                // Удаляем пустые складские позиции
                await _db.GetTable<ItemPositionModel>().Where(ip => ip.Quantity <= 0).DeleteAsync();

                // 6. Обновляем статус найденных заказов
                foreach (var orderId in orderIdsToUpdate)
                {
                    var order = await _db.GetTable<OrderModel>()
                        .FirstOrDefaultAsync(o => o.OrderId == orderId);

                    if (order != null && order.Status != OrderStatus.Completed.ToString())
                    {
                        order.Status = OrderStatus.Ready.ToString();
                        await _db.UpdateAsync(order);
                    }
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
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
            if (assignment == null) throw new ArgumentException("Assignment not found");

            if (assignment.Lines.Any(l => l.Status == OrderAssemblyLineStatus.Pending || l.Status == OrderAssemblyLineStatus.Picked))
                throw new InvalidOperationException("Не все товары размещены.");

            assignment.Complete(DateTime.UtcNow);
            await _assignmentRepo.UpdateAsync(assignment);

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();

            try
            {
                // 1. Освобождаем ячейки PICKUP (Исключаем NULL для экспресс-заказов)
                var targetCellIds = assignment.Lines
                    .Where(l => l.TargetPositionId.HasValue)
                    .Select(l => l.TargetPositionId.Value)
                    .Distinct()
                    .ToList();

                if (targetCellIds.Any())
                {
                    await _db.GetTable<PositionModel>()
                        .Where(p => targetCellIds.Contains(p.PositionId))
                        .Set(p => p.Status, "Active")
                        .UpdateAsync();
                }

                // 2. Обновляем статус заказа
                await _db.GetTable<OrderModel>()
                    .Where(o => o.OrderId == assignment.OrderId)
                    .Set(o => o.Status, OrderStatus.Ready.ToString())
                    .UpdateAsync();

                // 3. Обновляем базовую задачу
                await _db.GetTable<BaseTaskModel>()
                    .Where(t => t.TaskId == assignment.TaskId)
                    .Set(t => t.Status, "Completed")
                    .Set(t => t.CompletedAt, DateTime.UtcNow)
                    .UpdateAsync();

                // 4. Перемещение товаров
                var itemPositions = _db.GetTable<ItemPositionModel>();

                foreach (var line in assignment.Lines.Where(l => l.Status == OrderAssemblyLineStatus.Placed))
                {
                    // Фиксируем передвижение (если в БД поле DestinationPositionId осталось int, записываем 0 для экспресса)
                    await _db.InsertAsync(new ItemMovementModel
                    {
                        SourceItemPositionId = line.ItemPositionId,
                        DestinationPositionId = line.TargetPositionId ?? 0,
                        Quantity = line.Quantity,
                        CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                    });

                    // Уменьшаем остаток на исходной полке
                    await itemPositions
                        .Where(ip => ip.Id == line.ItemPositionId)
                        .Set(ip => ip.Quantity, ip => ip.Quantity - line.Quantity)
                        .UpdateAsync();

                    if (line.TargetPositionId.HasValue)
                    {
                        // Стандартная выдача: перекладываем товар в целевую ячейку
                        var originalItem = await itemPositions.FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId);
                        if (originalItem != null)
                        {
                            var newPosId = await _db.InsertWithInt32IdentityAsync(new ItemPositionModel
                            {
                                ItemId = originalItem.ItemId,
                                PositionId = line.TargetPositionId.Value, // ЯВНОЕ ПРИВЕДЕНИЕ
                                Quantity = line.Quantity,
                                CreatedAt = DateTime.Now
                            });

                            // Перепривязываем резерв к новой ячейке
                            await _db.GetTable<OrderReservationModel>()
                                .Where(r => r.ItemPositionId == line.ItemPositionId)
                                .Set(r => r.ItemPositionId, newPosId)
                                .UpdateAsync();
                        }
                    }
                    else
                    {
                        // Экспресс-выдача: отвязываем резерв от склада (товар в руках клиента)
                        await _db.GetTable<OrderReservationModel>()
                            .Where(r => r.ItemPositionId == line.ItemPositionId)
                            .Set(r => r.ItemPositionId, (int?)null)
                            .UpdateAsync();
                    }
                }

                // Удаляем обнуленные позиции со склада
                await itemPositions.Where(ip => ip.Quantity <= 0).DeleteAsync();

                // 5. Внедрение отчетов
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