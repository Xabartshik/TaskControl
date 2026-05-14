using Hangfire.Server;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.DataAccess.Model; // Для EmployeeModel
using TaskControl.InventoryModule.DataAccess.Model;   // Для PositionModel, ItemPositionModel, ItemModel
using TaskControl.OrderModule.DataAccess.Model;       // Для OrderModel
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Providers
{
    public class OrderHandoverExecutionProvider : ITaskExecutionProvider
    {
        private readonly ITaskDataConnection _db;
        private readonly IBaseTaskService _baseTaskService;
        private readonly ILogger<OrderHandoverExecutionProvider> _logger;
        private readonly ReturnTaskGeneratorService _returnTaskGenerator; 

        public string TaskType => "OrderHandover"; // Определяет, какие задачи падают сюда

        private readonly IQRTokenService _qrTokenService;
        private readonly ITaskComplexityCalculator _complexityCalculator;
        private readonly TaskWorkloadAggregator _aggregator;
        private readonly IOrderCancellationService _cancellationService;

        public OrderHandoverExecutionProvider(
            ITaskDataConnection db,
            IBaseTaskService baseTaskService,
            ILogger<OrderHandoverExecutionProvider> logger,
            ReturnTaskGeneratorService returnTaskGenerator,
            ITaskComplexityCalculator complexityCalculator,
            TaskWorkloadAggregator aggregator,
            IQRTokenService qrTokenService,
            IOrderCancellationService cancellationService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _baseTaskService = baseTaskService ?? throw new ArgumentNullException(nameof(baseTaskService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _complexityCalculator = complexityCalculator ?? throw new ArgumentNullException(nameof(complexityCalculator));
            _qrTokenService = qrTokenService ?? throw new ArgumentNullException(nameof(qrTokenService));
            _returnTaskGenerator = returnTaskGenerator ?? throw new ArgumentNullException(nameof(returnTaskGenerator));
            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
            _cancellationService = cancellationService;
        }
        // 1. СОХРАНЯЕМ ОТМЕНЫ В БД
        public async Task<bool> TryCompleteAssignmentAsync(int taskId, int workerId, Dictionary<int, int>? cancelledLines = null)
        {
            // Проверяем, относится ли задача к данному типу
            var baseTask = await _db.GetTable<BaseTaskModel>()
                                     .FirstOrDefaultAsync(t => t.TaskId == taskId && t.Type == this.TaskType);
            if (baseTask == null) return false;

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                // 1. Получаем все назначения задачи
                var allAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                    .Where(a => a.TaskId == taskId)
                    .ToListAsync();

                var assignment = allAssignments.FirstOrDefault(a => a.AssignedToUserId == workerId);
                if (assignment == null) return false;

                // 2. Получаем линии выдачи и определяем OrderId
                var handoverLines = await _db.GetTable<OrderHandoverLineModel>()
                    .Where(l => l.OrderHandoverAssignmentId == assignment.Id)
                    .ToListAsync();

                if (!handoverLines.Any()) return false;

                // Определяем OrderId через первую попавшуюся позицию
                var firstLine = handoverLines.First();
                var firstPos = await _db.GetTable<OrderPositionModel>()
                    .FirstOrDefaultAsync(p => p.UniqueId == firstLine.OrderPositionId);

                if (firstPos == null) return false;
                int orderId = firstPos.OrderId;

                var itemsToReturn = new List<(int ItemPositionId, int Qty)>();

                // === 3. ЛОГИКА ОТМЕНЫ ЧЕРЕЗ ЕДИНЫЙ СЕРВИС ===
                // === 3. ЛОГИКА ОТМЕНЫ ЧЕРЕЗ ЕДИНЫЙ СЕРВИС ===
                if (cancelledLines != null && cancelledLines.Any())
                {
                    var positionsToCancel = new Dictionary<int, int>();
                    int totalPicked = handoverLines.Sum(l => l.Quantity);
                    int totalCancelled = 0;

                    foreach (var kvp in cancelledLines)
                    {
                        int lineId = kvp.Key;
                        int rejectedQty = kvp.Value;

                        var line = handoverLines.FirstOrDefault(l => l.Id == lineId);
                        if (line == null || rejectedQty <= 0) continue;

                        int qty = Math.Min(rejectedQty, line.Quantity);
                        totalCancelled += qty;

                        // --- ИСПРАВЛЕНИЕ: Достаем физический ItemPositionId из резерва ---
                        var reservation = await _db.GetTable<OrderReservationModel>()
                            .FirstOrDefaultAsync(r => r.OrderPositionId == line.OrderPositionId);

                        if (reservation != null && reservation.ItemPositionId.HasValue)
                        {
                            int itemPosId = reservation.ItemPositionId.Value; // Безопасно извлекаем int

                            if (positionsToCancel.ContainsKey(itemPosId))
                                positionsToCancel[itemPosId] += qty;
                            else
                                positionsToCancel[itemPosId] = qty;
                        }
                        // ------------------------------------------------------------------

                        // СОХРАНЯЕМ ЛОГИКУ МОДУЛЯ ВЫДАЧИ: обновляем статистику в строке выдачи
                        await _db.GetTable<OrderHandoverLineModel>()
                            .Where(l => l.Id == line.Id)
                            .Set(l => l.CancelledQuantity, qty)
                            .Set(l => l.ScannedQuantity, l =>
                                 (l.Quantity - qty < l.ScannedQuantity)
                                 ? (l.Quantity - qty)
                                 : l.ScannedQuantity)
                            .UpdateAsync();
                    }

                    // Определяем, полная это отмена или частичная
                    bool isFullCancellation = totalPicked > 0 && totalPicked == totalCancelled;

                    // ВЫЗОВ ЕДИНОГО СЕРВИСА: он сам снимет резервы, поправит чек и сменит статус заказа
                    itemsToReturn = await _cancellationService.ProcessCancellationAsync(orderId, positionsToCancel, isFullCancellation);
                }
                else
                {
                    // Если отмен нет, просто завершаем заказ как Completed
                    await _db.GetTable<OrderModel>()
                        .Where(o => o.OrderId == orderId)
                        .Set(o => o.Status, "Completed")
                        .UpdateAsync();
                }

                // 4. Генерация задач на возврат
                if (itemsToReturn.Any())
                {
                    await _returnTaskGenerator.GenerateReturnTaskFromCancelledItemsAsync(orderId, baseTask.BranchId, itemsToReturn);
                }

                var completionTime = DateTime.UtcNow;

                // 5. Финализируем назначение текущего сотрудника
                await _db.GetTable<OrderHandoverAssignmentModel>()
                    .Where(a => a.Id == assignment.Id)
                    .Set(a => a.Status, 2) // Completed
                    .Set(a => a.CompletedAt, completionTime)
                    .UpdateAsync();

                // 6. АВТО-ЗАКРЫТИЕ ПОМОЩНИКА (только если закрывается Основной)
                if (assignment.Role == "Main")
                {
                    var activeHelpers = allAssignments.Where(a => a.Role == "Helper" && a.Status != 2);
                    if (activeHelpers.Any())
                    {
                        await _db.GetTable<OrderHandoverAssignmentModel>()
                            .Where(a => a.TaskId == taskId && a.Role == "Helper")
                            .Set(a => a.Status, 2)
                            .Set(a => a.CompletedAt, completionTime)
                            .UpdateAsync();

                        _logger.LogInformation("Помощники задачи {TaskId} закрыты автоматически вслед за основным сотрудником", taskId);
                    }
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при завершении выдачи {TaskId} для воркера {WorkerId}", taskId, workerId);
                return false;
            }
        }
        // 2. БЕРЕМ В ОБРАБОТКУ И ВЫДАННЫЕ, И ОТМЕНЕННЫЕ СТРОКИ
        public async Task ExecutePostCompletionLogicAsync(int taskId)
        {
            _logger.LogInformation("Запуск пост-обработки для задачи выдачи TaskId: {TaskId}", taskId);

            var assignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId).ToListAsync();

            if (!assignments.Any()) return;

            var assignmentIds = assignments.Select(a => a.Id).ToList();

            // Грузим строки, где есть ИЛИ сканы (выдано), ИЛИ отмены (клиент отказался)
            var allLines = await _db.GetTable<OrderHandoverLineModel>()
                .Where(l => assignmentIds.Contains(l.OrderHandoverAssignmentId) && (l.ScannedQuantity > 0 || l.CancelledQuantity > 0))
                .ToListAsync();

            if (!allLines.Any()) return;

            var linesLookup = allLines.ToLookup(l => l.OrderHandoverAssignmentId);

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                foreach (var assignment in assignments)
                {
                    var currentOrderLines = linesLookup[assignment.Id].ToList();
                    if (!currentOrderLines.Any()) continue;

                    // ВОЗВРАЩЕНО: Проверка целостности исполнителя
                    if (!assignment.AssignedToUserId.HasValue)
                    {
                        throw new InvalidOperationException($"Ошибка целостности: у назначения {assignment.Id} нет исполнителя!");
                    }

                    var workerId = assignment.AssignedToUserId.Value;

                    if (assignment.HandoverType == "ToCustomer")
                    {
                        await ProcessHandoverToCustomerAsync(assignment.OrderId, currentOrderLines, taskId, workerId);
                    }
                    else if (assignment.HandoverType == "ToCourier")
                    {
                        // ВОЗВРАЩЕНО: Проверка ID курьера
                        if (assignment.TargetCourierId == null)
                        {
                            throw new InvalidOperationException($"Не указан ID курьера для заказа {assignment.OrderId}!");
                        }
                        await ProcessHandoverToCourierAsync(assignment.OrderId, assignment.TargetCourierId.Value, currentOrderLines, taskId, workerId);
                    }
                }
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при пост-обработке задачи {TaskId}", taskId);
                throw;
            }
        }

        // 3. ВЫДАЧА КЛИЕНТУ: УМНОЕ СПИСАНИЕ
        private async Task ProcessHandoverToCustomerAsync(int orderId, List<OrderHandoverLineModel> lines, int taskId, int workerId)
        {
            var itemPositions = _db.GetTable<ItemPositionModel>();
            var reservations = _db.GetTable<OrderReservationModel>();

            foreach (var line in lines)
            {
                if (line.ItemPositionId == null) continue;

                var sourceItemPos = await itemPositions.FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId.Value);
                if (sourceItemPos == null) continue;

                // Снимаем резерв конкретно нашего заказа (в любом случае)
                await reservations.Where(r => r.OrderPositionId == line.OrderPositionId).DeleteAsync();

                // Если товар был реально выдан (отсканирован):
                if (line.ScannedQuantity > 0)
                {
                    var remainingQty = sourceItemPos.Quantity - line.ScannedQuantity;

                    if (remainingQty <= 0)
                    {
                        // ВОЗВРАЩЕНО (Self-Healing): Отвязываем чужие резервы и другие строки задач от пустой ячейки
                        await reservations
                            .Where(r => r.ItemPositionId == sourceItemPos.Id)
                            .Set(r => r.ItemPositionId, (int?)null)
                            .UpdateAsync();

                        await _db.GetTable<OrderHandoverLineModel>()
                            .Where(l => l.ItemPositionId == sourceItemPos.Id)
                            .Set(l => l.ItemPositionId, (int?)null)
                            .UpdateAsync();

                        // Теперь безопасно удаляем ячейку
                        await itemPositions.Where(ip => ip.Id == sourceItemPos.Id).DeleteAsync();
                    }
                    else
                    {
                        await itemPositions.Where(ip => ip.Id == sourceItemPos.Id).Set(ip => ip.Quantity, remainingQty).UpdateAsync();
                    }

                    // Логируем движение (выход со склада в никуда/клиенту)
                    await _db.InsertAsync(new ItemMovementModel
                    {
                        ItemId = sourceItemPos.ItemId,
                        SourcePositionId = remainingQty <= 0 ? (int?)null : sourceItemPos.PositionId,
                        DestinationPositionId = null,
                        Quantity = line.ScannedQuantity,
                        TaskId = taskId,
                        WorkerId = workerId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                // Если была только отмена (line.CancelledQuantity > 0), товар просто остается в sourceItemPos без резерва.
            }

            // Учет отмен при определении итогового статуса заказа
            bool hasScans = lines.Any(l => l.ScannedQuantity > 0);
            bool hasCancellations = lines.Any(l => l.CancelledQuantity > 0);

            string newStatus = "Completed";
            if (hasCancellations && hasScans) newStatus = "PartiallyCompleted";
            else if (hasCancellations && !hasScans) newStatus = "Cancelled";

            await _db.GetTable<OrderModel>().Where(o => o.OrderId == orderId).Set(o => o.Status, newStatus).UpdateAsync();
        }



        // 4. ВЫДАЧА КУРЬЕРУ (Аналогично)
        private async Task ProcessHandoverToCourierAsync(int orderId, int courierId, List<OrderHandoverLineModel> lines, int taskId, int workerId)
        {
            // ВОЗВРАЩЕНО: Проверка наличия виртуальной ячейки курьера
            var courierPosition = await _db.GetTable<PositionModel>()
                .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == courierId.ToString());

            if (courierPosition == null)
                throw new InvalidOperationException($"Виртуальная ячейка для курьера ID {courierId} не найдена!");

            var itemPositions = _db.GetTable<ItemPositionModel>();
            var reservations = _db.GetTable<OrderReservationModel>();

            foreach (var line in lines)
            {
                if (line.ItemPositionId == null) continue;

                var sourceItemPos = await itemPositions.FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId.Value);
                if (sourceItemPos == null) continue;

                // Передаем курьеру ТОЛЬКО отсканированное
                if (line.ScannedQuantity > 0)
                {
                    var courierItemPos = await itemPositions.FirstOrDefaultAsync(ip => ip.PositionId == courierPosition.PositionId && ip.ItemId == sourceItemPos.ItemId);
                    int newCourierItemPosId;

                    if (courierItemPos != null)
                    {
                        await itemPositions.Where(ip => ip.Id == courierItemPos.Id).Set(ip => ip.Quantity, ip => ip.Quantity + line.ScannedQuantity).UpdateAsync();
                        newCourierItemPosId = courierItemPos.Id;
                    }
                    else
                    {
                        newCourierItemPosId = await _db.InsertWithInt32IdentityAsync(new ItemPositionModel
                        {
                            ItemId = sourceItemPos.ItemId,
                            PositionId = courierPosition.PositionId,
                            Quantity = line.ScannedQuantity,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Перепривязываем резерв на "багажник" курьера
                    await reservations.Where(r => r.OrderPositionId == line.OrderPositionId).Set(r => r.ItemPositionId, newCourierItemPosId).UpdateAsync();

                    var remainingSourceQty = sourceItemPos.Quantity - line.ScannedQuantity;
                    if (remainingSourceQty <= 0)
                    {
                        // ВОЗВРАЩЕНО (Self-Healing): Очистка пустой складской полки
                        await reservations.Where(r => r.ItemPositionId == sourceItemPos.Id).Set(r => r.ItemPositionId, (int?)null).UpdateAsync();
                        await _db.GetTable<OrderHandoverLineModel>().Where(l => l.ItemPositionId == sourceItemPos.Id).Set(l => l.ItemPositionId, (int?)null).UpdateAsync();
                        await itemPositions.Where(ip => ip.Id == sourceItemPos.Id).DeleteAsync();
                    }
                    else
                    {
                        await itemPositions.Where(ip => ip.Id == sourceItemPos.Id).Set(ip => ip.Quantity, remainingSourceQty).UpdateAsync();
                    }

                    await _db.InsertAsync(new ItemMovementModel
                    {
                        ItemId = sourceItemPos.ItemId,
                        SourcePositionId = sourceItemPos.PositionId,
                        DestinationPositionId = courierPosition.PositionId,
                        Quantity = line.ScannedQuantity,
                        TaskId = taskId,
                        WorkerId = workerId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else if (line.CancelledQuantity > 0)
                {
                    // Товар отменили. Снимаем резерв заказа со складской полки.
                    await reservations.Where(r => r.OrderPositionId == line.OrderPositionId).DeleteAsync();
                }
            }

            bool hasScans = lines.Any(l => l.ScannedQuantity > 0);
            string newStatus = hasScans ? "InTransit" : "Cancelled";

            await _db.GetTable<OrderModel>().Where(o => o.OrderId == orderId).Set(o => o.Status, newStatus).UpdateAsync();
        }

        // ==========================================
        // 1. ИНФОРМАЦИЯ ДЛЯ МОБИЛЬНОГО ПРИЛОЖЕНИЯ
        // ==========================================
        public async Task<(bool Success, string Message)> TryCompleteWithCourierQrAsync(int taskId, int workerId, string qrToken)
        {
            _logger.LogInformation("Попытка закрытия пакетной отгрузки {TaskId} по QR-коду", taskId);

            // 1. Расшифровываем и проверяем QR-код
            if (!_qrTokenService.ValidateCourierPickupToken(qrToken, out int courierIdFromQr, out string errorMessage))
            {
                return (false, errorMessage);
            }

            // 2. Достаем назначения, чтобы узнать, какому курьеру мы ДОЛЖНЫ были отдать груз
            var assignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId)
                .ToListAsync();

            if (!assignments.Any()) return (false, "Назначения не найдены.");

            var targetCourierId = assignments.FirstOrDefault()?.TargetCourierId;

            // 3. Сверяем ID из маршрутного листа с ID из QR-кода
            if (targetCourierId == null || targetCourierId != courierIdFromQr)
            {
                return (false, $"Отказано. QR-код принадлежит курьеру ID:{courierIdFromQr}, а маршрут назначен на курьера ID:{targetCourierId}");
            }

            // 4. Если всё совпало - завершаем задачу штатно
            bool completed = await TryCompleteAssignmentAsync(taskId, workerId);

            // --- ИСПРАВЛЕНИЕ: ВРУЧНУЮ ЗАПУСКАЕМ ПОСТ-ОБРАБОТКУ ---
            if (completed)
            {
                // Проверяем, закрылись ли все части задачи (Главный + Помощник)
                bool isFullyCompleted = await IsTaskFullyCompletedAsync(taskId);

                if (isFullyCompleted)
                {
                    // Обновляем базовую задачу, как это делает стандартный контроллер
                    await _db.GetTable<BaseTaskModel>()
                        .Where(t => t.TaskId == taskId)
                        .Set(t => t.Status, "Completed")
                        .Set(t => t.CompletedAt, DateTime.UtcNow)
                        .UpdateAsync();

                    // Запускаем магию: списываем склад, переводим заказы в InTransit!
                    await ExecutePostCompletionLogicAsync(taskId);
                }
            }
            // -----------------------------------------------------

            return (completed, completed ? "Отгрузка курьеру подтверждена!" : "Ошибка при закрытии задачи в БД.");
        }

        public async Task<object?> GetTaskDetailsAsync(int taskId, int workerId)
        {
            _logger.LogInformation("Получение деталей задачи выдачи TaskId: {TaskId} для WorkerId: {WorkerId}", taskId, workerId);

            var allAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                                          .Where(a => a.TaskId == taskId)
                                          .ToListAsync();

            // БЕРЕМ ВСЕ НАЗНАЧЕНИЯ ТЕКУЩЕГО РАБОТНИКА (в маршруте их может быть 10 штук)
            var workerAssignments = allAssignments.Where(a => a.AssignedToUserId == workerId).ToList();
            var currentAssignment = workerAssignments.FirstOrDefault();

            if (currentAssignment == null)
            {
                _logger.LogWarning("Назначение на выдачу не найдено для TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
                return null;
            }

            var baseTask = await _baseTaskService.GetById(taskId);

            // ИСПРАВЛЕНИЕ 1: Настоящий напарник - это человек с ДРУГИМ ID
            var partnerAssignment = allAssignments.FirstOrDefault(a =>
                a.AssignedToUserId != null &&
                a.AssignedToUserId != workerId);

            bool isCooperative = partnerAssignment != null;
            string partnerName = null;

            if (isCooperative)
            {
                var partnerUser = await _db.GetTable<EmployeeModel>().FirstOrDefaultAsync(u => u.EmployeesId == partnerAssignment.AssignedToUserId);
                partnerName = partnerUser?.FullName ?? $"Сотрудник ID: {partnerAssignment.AssignedToUserId}";
            }

            var dto = new HandoverTaskDetailsDto
            {
                AssignmentId = currentAssignment.Id,
                TaskId = currentAssignment.TaskId,
                TaskNumber = baseTask?.Title ?? $"Выдача заказа #{currentAssignment.OrderId}",
                OrderId = currentAssignment.OrderId,
                HandoverType = currentAssignment.HandoverType,
                Status = currentAssignment.Status,
                IsCooperative = isCooperative,
                PartnerName = partnerName,
                PartnerStatus = partnerAssignment?.Status
            };

            if (currentAssignment.HandoverType == "ToCourier" && currentAssignment.TargetCourierId.HasValue)
            {
                var courierInfo = await _db.GetTable<EmployeeModel>()
                    .FirstOrDefaultAsync(u => u.EmployeesId == currentAssignment.TargetCourierId.Value);
                dto.TargetName = courierInfo?.FullName ?? $"Курьер ID: {currentAssignment.TargetCourierId.Value}";
            }
            else
            {
                dto.TargetName = "Покупатель (Самовывоз)";
            }

            // ИСПРАВЛЕНИЕ 2: Собираем товары со ВСЕХ заказов в маршруте!
            var workerAssignmentIds = workerAssignments.Select(a => a.Id).ToList();
            var lines = await _db.GetTable<OrderHandoverLineModel>()
                                 .Where(l => workerAssignmentIds.Contains(l.OrderHandoverAssignmentId))
                                 .ToListAsync();

            var positions = _db.GetTable<PositionModel>();
            var itemPositions = _db.GetTable<ItemPositionModel>();
            var items = _db.GetTable<ItemModel>();

            foreach (var line in lines)
            {
                var itemInfoQuery = from ip in itemPositions
                                    join i in items on ip.ItemId equals i.ItemId
                                    where ip.Id == line.ItemPositionId
                                    select new { i.ItemId, i.Name, i.Barcode, ip.PositionId };

                var itemInfo = await itemInfoQuery.FirstOrDefaultAsync();

                string sourceCellCode = "Неизвестная ячейка";
                if (itemInfo != null)
                {
                    var sourcePosModel = await positions.FirstOrDefaultAsync(p => p.PositionId == itemInfo.PositionId);
                    sourceCellCode = GetFullPositionCode(sourcePosModel) ?? itemInfo.PositionId.ToString();
                }

                var orderPos = await _db.GetTable<OrderPositionModel>()
                    .FirstOrDefaultAsync(op => op.UniqueId == line.OrderPositionId);

                dto.ItemsToScan.Add(new HandoverItemDto
                {
                    LineId = line.Id,
                    ItemId = itemInfo?.ItemId ?? 0,
                    ItemName = itemInfo?.Name ?? "Неизвестный товар",
                    Barcode = itemInfo?.Barcode ?? "Неизвестный штрих-код",
                    SourceCellCode = sourceCellCode,
                    Quantity = line.Quantity,
                    ScannedQuantity = line.ScannedQuantity,
                    Price = orderPos?.Price ?? 0
                });
            }

            return dto;
        }


        // Вспомогательный метод для форматирования кода ячейки
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

        // ==========================================
        // 1. МЕТОДЫ УПРАВЛЕНИЯ СТАТУСОМ ЗАДАЧИ
        // ==========================================
        public async Task<(bool Success, string Message)> ProcessScanAsync(int taskId, int workerId, string barcode)
        {
            _logger.LogInformation("Сканирование при выдаче. TaskId: {TaskId}, Barcode: {Barcode}", taskId, barcode);

            // 1. Ищем активное назначение
            var workerAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                            .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId && a.Status == 1)
                            .ToListAsync();

            if (!workerAssignments.Any())
                return (false, "Активная задача выдачи не найдена или не в статусе InProgress.");

            // --- ИСПРАВЛЕНИЕ ШТРИХ-КОДА ---
            // Убрали int.TryParse. Теперь ищем товар по строке Barcode
            var scannedItem = await _db.GetTable<ItemModel>()
                .FirstOrDefaultAsync(i => i.Barcode == barcode.Trim());

            if (scannedItem == null)
                return (false, $"Товар со штрих-кодом '{barcode}' не найден в справочнике системы.");

            int scannedItemId = scannedItem.ItemId; // Запоминаем ID найденного товара
            // -----------------------------

            var assignmentIds = workerAssignments.Select(a => a.Id).ToList();

            // 2. Ищем строки задания по ВСЕМ назначениям пакета
            var lines = await _db.GetTable<OrderHandoverLineModel>()
                .Where(l => assignmentIds.Contains(l.OrderHandoverAssignmentId))
                .ToListAsync();

            var itemPositions = _db.GetTable<ItemPositionModel>();

            // 3. Ищем товар, который еще не до конца отсканирован
            OrderHandoverLineModel targetLine = null;
            foreach (var line in lines)
            {
                if (line.ScannedQuantity >= line.Quantity) continue; // Этот уже собран

                var ip = await itemPositions.FirstOrDefaultAsync(x => x.Id == line.ItemPositionId);

                // Сравниваем ItemId ячейки с ItemId, который мы получили из штрих-кода
                if (ip != null && ip.ItemId == scannedItemId)
                {
                    targetLine = line;
                    break;
                }
            }

            if (targetLine == null)
                return (false, "Товар не из этого заказа или уже полностью отсканирован!");

            // 4. Увеличиваем счетчик
            await _db.GetTable<OrderHandoverLineModel>()
                .Where(l => l.Id == targetLine.Id)
                .Set(l => l.ScannedQuantity, l => l.ScannedQuantity + 1)
                .UpdateAsync();

            // 5. Проверяем, завершили ли мы выдачу всех товаров
            bool isAllScanned = lines.All(l =>
                (l.Id == targetLine.Id ? l.ScannedQuantity + 1 : l.ScannedQuantity) >= l.Quantity);

            if (isAllScanned)
            {
                return (true, "FINISH:Все товары отсканированы! Можете завершать задачу.");
            }

            return (true, "Товар успешно отсканирован.");
        }
        public async Task<bool> TryActivateTaskAsync(int taskId, int workerId)
        {
            _logger.LogInformation("Попытка активации задачи выдачи {TaskId} сотрудником {WorkerId}", taskId, workerId);

            await PauseActiveTasksAsync(workerId, taskId);

            var allAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId)
                .ToListAsync();

            // Ищем ВСЕ назначения этого работника в рамках данной задачи (их может быть много при пакетной отгрузке)
            var workerAssignments = allAssignments.Where(a => a.AssignedToUserId == workerId).ToList();
            if (!workerAssignments.Any())
            {
                _logger.LogWarning("Не удалось найти назначение для задачи {TaskId} у пользователя {WorkerId}", taskId, workerId);
                return false;
            }

            // 1. Отмечаем ВСЕ назначения текущего сотрудника как InProgress
            await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId)
                .Set(a => a.Status, 1) // 1 = InProgress
                .Set(a => a.StartedAt, DateTime.UtcNow)
                .UpdateAsync();

            // 2. Ищем напарника (все назначения чужих пользователей)
            var partnerAssignments = allAssignments.Where(a => a.AssignedToUserId != workerId && a.AssignedToUserId != null).ToList();

            // Если напарников нет, ИЛИ ВСЕ их назначения уже в работе (Status 1 или 2)
            bool partnerReady = !partnerAssignments.Any() || partnerAssignments.All(a => a.Status == 1 || a.Status == 2);

            if (partnerReady)
            {
                await _db.GetTable<BaseTaskModel>()
                    .Where(t => t.TaskId == taskId && t.Status != "InProgress")
                    .Set(t => t.Status, "InProgress")
                    .UpdateAsync();
            }

            // ВАЖНО: Всегда возвращаем true! Мы успешно записали свой статус в БД.
            // Мобилка получит 200 OK, скачает новые статусы и сама отрисует экран ожидания напарника.
            return true;
        }

        //public async Task<bool> TryCompleteAssignmentAsync(int taskId, int workerId)
        //{
        //    _logger.LogInformation("Сотрудник {WorkerId} завершил свою часть выдачи {TaskId}", taskId, workerId);

        //    var allAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
        //        .Where(a => a.TaskId == taskId)
        //        .ToListAsync();

        //    var workerAssignments = allAssignments.Where(a => a.AssignedToUserId == workerId).ToList();
        //    if (!workerAssignments.Any()) return false;

        //    var completionTime = DateTime.UtcNow;

        //    // 1. Завершаем работу текущего сотрудника (сразу по всем заказам в маршруте)
        //    await _db.GetTable<OrderHandoverAssignmentModel>()
        //        .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId)
        //        .Set(a => a.Status, 2) // 2 = Completed
        //        .Set(a => a.CompletedAt, completionTime)
        //        .UpdateAsync();

        //    // 2. АВТО-ЗАКРЫТИЕ: Если кнопку нажал Главный, гасим Помощника
        //    if (workerAssignments.Any(a => a.Role == "Main"))
        //    {
        //        var helperAssignments = allAssignments.Where(a => a.Role == "Helper" && a.Status != 2).ToList();
        //        if (helperAssignments.Any())
        //        {
        //            // Гасим сразу все строки помощника
        //            await _db.GetTable<OrderHandoverAssignmentModel>()
        //                .Where(a => a.TaskId == taskId && a.Role == "Helper")
        //                .Set(a => a.Status, 2)
        //                .Set(a => a.CompletedAt, completionTime)
        //                .UpdateAsync();

        //            _logger.LogInformation("Назначения помощника для пакетной задачи {TaskId} автоматически закрыты", taskId);
        //        }
        //    }

        //    return true;
        //}


        public async Task<bool> TryPauseTaskAsync(int taskId, int workerId)
        {
            _logger.LogInformation("Постановка на паузу задачи выдачи {TaskId} сотрудником {WorkerId}", taskId, workerId);

            var updated = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId)
                .Set(a => a.Status, 0) // Возвращаем в Assigned (0) или используем специальный статус паузы
                .UpdateAsync();

            return updated > 0;
        }

        public async Task<bool> TryCancelTaskAsync(int taskId, int workerId)
        {
            _logger.LogInformation("Отмена назначения на выдачу {TaskId} для сотрудника {WorkerId}", taskId, workerId);

            var updated = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId)
                .Set(a => a.Status, 3) // 3 = Cancelled
                .UpdateAsync();

            return updated > 0;
        }
        public async Task<bool> IsTaskFullyCompletedAsync(int taskId)
        {
            // Проверяем статус всех назначений в рамках одной выдачи
            var assignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId)
                .Select(a => a.Status)
                .ToListAsync();

            if (!assignments.Any()) return false;

            // Считаем задачу полностью выполненной, если ВСЕ назначения либо Completed (2), либо Cancelled (3)
            return assignments.All(status => status == 2 || status == 3);
        }

        public async Task PauseActiveTasksAsync(int workerId, int excludeTaskId)
        {
            // Находим все активные задачи (Status = 1) сотрудника, кроме текущей, и ставим их на паузу
            var pausedCount = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.AssignedToUserId == workerId && a.Status == 1 && a.TaskId != excludeTaskId)
                .Set(a => a.Status, 0)
                .UpdateAsync();

            if (pausedCount > 0)
            {
                _logger.LogInformation("Поставлено на паузу фоновых задач выдачи: {Count} для сотрудника {WorkerId}", pausedCount, workerId);
            }
        }
        // ==========================================
        // 3. БИЗНЕС-ЛОГИКА И ПОСТ-ОБРАБОТКА (ЭТАП 3)
        // ==========================================

        //public async Task ExecutePostCompletionLogicAsync(int taskId)
        //{
        //    _logger.LogInformation("Запуск оптимизированной пост-обработки для задачи пакетной выдачи TaskId: {TaskId}", taskId);

        //    // 1. Пакетная загрузка всех назначений (заказов) в этой задаче
        //    var assignments = await _db.GetTable<OrderHandoverAssignmentModel>()
        //        .Where(a => a.TaskId == taskId)
        //        .ToListAsync();

        //    if (!assignments.Any())
        //    {
        //        _logger.LogWarning("Назначения для задачи {TaskId} не найдены", taskId);
        //        return;
        //    }

        //    // 2. Пакетная загрузка ВСЕХ строк для ВСЕХ заказов одним запросом (как в старом коде)
        //    var assignmentIds = assignments.Select(a => a.Id).ToList();
        //    var allLines = await _db.GetTable<OrderHandoverLineModel>()
        //        .Where(l => assignmentIds.Contains(l.OrderHandoverAssignmentId) && l.ScannedQuantity > 0)
        //        .ToListAsync();

        //    if (!allLines.Any())
        //    {
        //        _logger.LogWarning("Нет отсканированных товаров для задачи {TaskId}. Списание не требуется.", taskId);
        //        return;
        //    }

        //    // Группируем строки по ID назначения в памяти, чтобы не делать запросы в цикле
        //    var linesLookup = allLines.ToLookup(l => l.OrderHandoverAssignmentId);

        //    // 3. Работа в транзакции для обеспечения целостности данных
        //    using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
        //    try
        //    {
        //        foreach (var assignment in assignments)
        //        {
        //            // Извлекаем строки для конкретного заказа из нашего Lookup
        //            var currentOrderLines = linesLookup[assignment.Id].ToList();

        //            if (!currentOrderLines.Any())
        //                continue; // Пропускаем, если по этому заказу ничего не отсканировали

        //            // Проверка наличия исполнителя (безопасность из нового кода)
        //            if (!assignment.AssignedToUserId.HasValue)
        //            {
        //                throw new InvalidOperationException($"Ошибка целостности: у назначения {assignment.Id} нет исполнителя!");
        //            }

        //            var workerId = assignment.AssignedToUserId.Value;

        //            // Логика обработки в зависимости от типа выдачи
        //            if (assignment.HandoverType == "ToCustomer")
        //            {
        //                await ProcessHandoverToCustomerAsync(assignment.OrderId, currentOrderLines, taskId, workerId);
        //            }
        //            else if (assignment.HandoverType == "ToCourier")
        //            {
        //                if (assignment.TargetCourierId == null)
        //                    throw new InvalidOperationException($"Не указан ID курьера для заказа {assignment.OrderId}!");

        //                await ProcessHandoverToCourierAsync(assignment.OrderId, assignment.TargetCourierId.Value, currentOrderLines, taskId, workerId);
        //            }
        //        }

        //        await transaction.CommitAsync();
        //        _logger.LogInformation("Пост-обработка пакетной задачи {TaskId} успешно завершена", taskId);
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        _logger.LogError(ex, "Ошибка транзакции при пост-обработке задачи выдачи {TaskId}", taskId);
        //        throw;
        //    }
        //}

        //private async Task ProcessHandoverToCustomerAsync(int orderId, List<OrderHandoverLineModel> lines, int taskId, int workerId)
        //{
        //    var itemPositions = _db.GetTable<ItemPositionModel>();
        //    var reservations = _db.GetTable<OrderReservationModel>();
        //    var movements = _db.GetTable<ItemMovementModel>(); // Добавлено для логирования

        //    foreach (var line in lines)
        //    {
        //        if (line.ItemPositionId == null) continue;

        //        var sourceItemPos = await itemPositions.FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId.Value);
        //        if (sourceItemPos == null) continue;

        //        // А. СНАЧАЛА удаляем резерв конкретно нашего заказа
        //        await reservations
        //            .Where(r => r.OrderPositionId == line.OrderPositionId)
        //            .DeleteAsync();

        //        // Б. Считаем остаток
        //        var remainingQty = sourceItemPos.Quantity - line.ScannedQuantity;

        //        if (remainingQty <= 0)
        //        {
        //            // WMS SELF-HEALING 1: Отвязываем чужие резервы
        //            await reservations
        //                .Where(r => r.ItemPositionId == sourceItemPos.Id)
        //                .Set(r => r.ItemPositionId, (int?)null)
        //                .UpdateAsync();

        //            // WMS SELF-HEALING 2: Отвязываем строки задания выдачи (чтобы не нарушать FK)
        //            await _db.GetTable<OrderHandoverLineModel>()
        //                .Where(l => l.ItemPositionId == sourceItemPos.Id)
        //                .Set(l => l.ItemPositionId, (int?)null)
        //                .UpdateAsync();

        //            // Теперь база разрешит удалить пустую ячейку
        //            await itemPositions.Where(ip => ip.Id == sourceItemPos.Id).DeleteAsync();
        //        }
        //        else
        //        {
        //            await itemPositions
        //                .Where(ip => ip.Id == sourceItemPos.Id)
        //                .Set(ip => ip.Quantity, remainingQty)
        //                .UpdateAsync();
        //        }

        //        // В. Логируем перемещение
        //        await _db.InsertAsync(new ItemMovementModel
        //        {
        //            ItemId = sourceItemPos.ItemId,
        //            SourcePositionId = remainingQty <= 0 ? (int?)null : sourceItemPos.PositionId,
        //            DestinationPositionId = null,
        //            Quantity = line.ScannedQuantity,
        //            TaskId = taskId,       // ИСПОЛЬЗУЕМ ЗДЕСЬ
        //            WorkerId = workerId,   // И ЗДЕСЬ
        //            CreatedAt = DateTime.UtcNow
        //        });
        //    }

        //    await _db.GetTable<OrderModel>()
        //        .Where(o => o.OrderId == orderId)
        //        .Set(o => o.Status, "Completed")
        //        .UpdateAsync();
        //}


        //private async Task ProcessHandoverToCourierAsync(int orderId, int courierId, List<OrderHandoverLineModel> lines, int taskId, int workerId)
        //{
        //    var courierPosition = await _db.GetTable<PositionModel>()
        //        .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == courierId.ToString());

        //    if (courierPosition == null)
        //        throw new InvalidOperationException($"Виртуальная ячейка для курьера ID {courierId} не найдена!");

        //    var itemPositions = _db.GetTable<ItemPositionModel>();
        //    var reservations = _db.GetTable<OrderReservationModel>();
        //    var movements = _db.GetTable<ItemMovementModel>();

        //    foreach (var line in lines)
        //    {
        //        if (line.ItemPositionId == null) continue;

        //        var sourceItemPos = await itemPositions.FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId.Value);
        //        if (sourceItemPos == null) continue;

        //        // А. Ищем или создаем товар в багажнике курьера
        //        var courierItemPos = await itemPositions
        //            .FirstOrDefaultAsync(ip => ip.PositionId == courierPosition.PositionId && ip.ItemId == sourceItemPos.ItemId);

        //        int newCourierItemPosId;
        //        if (courierItemPos != null)
        //        {
        //            await itemPositions
        //                .Where(ip => ip.Id == courierItemPos.Id)
        //                .Set(ip => ip.Quantity, ip => ip.Quantity + line.ScannedQuantity)
        //                .UpdateAsync();
        //            newCourierItemPosId = courierItemPos.Id;
        //        }
        //        else
        //        {
        //            newCourierItemPosId = await _db.InsertWithInt32IdentityAsync(new ItemPositionModel
        //            {
        //                ItemId = sourceItemPos.ItemId,
        //                PositionId = courierPosition.PositionId,
        //                Quantity = line.ScannedQuantity,
        //                CreatedAt = DateTime.UtcNow
        //            });
        //        }

        //        // Б. Перепривязываем резерв нашего заказа в багажник
        //        await reservations
        //            .Where(r => r.OrderPositionId == line.OrderPositionId)
        //            .Set(r => r.ItemPositionId, newCourierItemPosId)
        //            .UpdateAsync();

        //        // В. Считаем остаток на старой полке
        //        var remainingSourceQty = sourceItemPos.Quantity - line.ScannedQuantity;

        //        if (remainingSourceQty <= 0)
        //        {
        //            // WMS SELF-HEALING 1: Очищаем старую пустую полку от резервов
        //            await reservations
        //                .Where(r => r.ItemPositionId == sourceItemPos.Id)
        //                .Set(r => r.ItemPositionId, (int?)null)
        //                .UpdateAsync();

        //            // WMS SELF-HEALING 2: Отвязываем строки задания выдачи
        //            await _db.GetTable<OrderHandoverLineModel>()
        //                .Where(l => l.ItemPositionId == sourceItemPos.Id)
        //                .Set(l => l.ItemPositionId, (int?)null)
        //                .UpdateAsync();

        //            // Удаляем пустую ячейку
        //            await itemPositions.Where(ip => ip.Id == sourceItemPos.Id).DeleteAsync();
        //        }
        //        else
        //        {
        //            await itemPositions
        //                .Where(ip => ip.Id == sourceItemPos.Id)
        //                .Set(ip => ip.Quantity, remainingSourceQty)
        //                .UpdateAsync();
        //        }

        //        // Г. Логируем
        //        // Г. Логируем перемещение
        //        await _db.InsertAsync(new ItemMovementModel
        //        {
        //            ItemId = sourceItemPos.ItemId, // <-- Пишем ID товара
        //            SourcePositionId = sourceItemPos.PositionId, // <-- Складская полка
        //            DestinationPositionId = courierPosition.PositionId, // <-- Полка багажника курьера
        //            Quantity = line.ScannedQuantity,
        //            TaskId = taskId,
        //            CreatedAt = DateTime.UtcNow
        //        });
        //    }

        //    await _db.GetTable<OrderModel>()
        //        .Where(o => o.OrderId == orderId)
        //        .Set(o => o.Status, "InTransit")
        //        .UpdateAsync();
        //}

        public async Task<bool> AssignTaskToWorkerAsync(int taskId, int workerId)
        {
            // 0. Вместо AnyAsync получаем саму задачу, чтобы иметь доступ к её свойствам (в т.ч. BranchId)
            var task = await _db.GetTable<BaseTaskModel>()
                                 .FirstOrDefaultAsync(t => t.TaskId == taskId && t.Type == this.TaskType);

            // Если задача не найдена или тип не совпадает — выходим
            if (task == null) return false;

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                // 1. Находим все неназначенные роли для этой задачи
                var assignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                    .Where(a => a.TaskId == taskId && a.AssignedToUserId == null)
                    .ToListAsync();

                if (!assignments.Any()) return false;

                // 2. Назначаем текущего сотрудника на роль "Main"
                var mainAssignment = assignments.FirstOrDefault(a => a.Role == "Main");
                if (mainAssignment == null) return false; // Кто-то уже перехватил роль главного

                await _db.GetTable<OrderHandoverAssignmentModel>()
                    .Where(a => a.Id == mainAssignment.Id)
                    .Set(a => a.AssignedToUserId, workerId)
                    .UpdateAsync();

                // 3. АВТОПОИСК ПОМОЩНИКА (Triggered by Main)
                var helperAssignment = assignments.FirstOrDefault(a => a.Role == "Helper");
                if (helperAssignment != null)
                {
                    // ИСПРАВЛЕНО: Теперь BranchId берется из объекта task (BaseTaskModel)
                    int? autoHelperId = await _aggregator.FindAvailableHelperAsync(task.BranchId, workerId);

                    if (autoHelperId.HasValue)
                    {
                        await _db.GetTable<OrderHandoverAssignmentModel>()
                            .Where(a => a.Id == helperAssignment.Id)
                            .Set(a => a.AssignedToUserId, autoHelperId.Value)
                            .UpdateAsync();

                        _logger.LogInformation("Для задачи {TaskId} автоматически назначен помощник {HelperId}", taskId, autoHelperId);
                    }
                    else
                    {
                        _logger.LogInformation("Свободных помощников для задачи {TaskId} не найдено. Вакансия остается в пуле.", taskId);
                    }
                }

                // 4. Обновляем статус базовой задачи
                await _db.GetTable<BaseTaskModel>()
                    .Where(t => t.TaskId == taskId)
                    .Set(t => t.Status, "Assigned")
                    .UpdateAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при динамическом назначении помощника для задачи {TaskId}", taskId);
                return false;
            }
        }
    }
}