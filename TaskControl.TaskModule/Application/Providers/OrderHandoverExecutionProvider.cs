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

        public string TaskType => "OrderHandover"; // Определяет, какие задачи падают сюда

        private readonly IQRTokenService _qrTokenService; 

        public OrderHandoverExecutionProvider(
            ITaskDataConnection db,
            IBaseTaskService baseTaskService,
            ILogger<OrderHandoverExecutionProvider> logger,
            IQRTokenService qrTokenService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _baseTaskService = baseTaskService ?? throw new ArgumentNullException(nameof(baseTaskService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _qrTokenService = qrTokenService ?? throw new ArgumentNullException(nameof(qrTokenService));
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
                                    select new { i.ItemId, i.Name, ip.PositionId };

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
                    Barcode = (itemInfo?.ItemId ?? 0).ToString(),
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

            if (!int.TryParse(barcode, out int itemId))
                return (false, "Неверный формат штрих-кода.");

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
                if (ip != null && ip.ItemId == itemId)
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

        public async Task<bool> TryCompleteAssignmentAsync(int taskId, int workerId)
        {
            _logger.LogInformation("Сотрудник {WorkerId} завершил свою часть выдачи {TaskId}", taskId, workerId);

            var allAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId)
                .ToListAsync();

            var workerAssignments = allAssignments.Where(a => a.AssignedToUserId == workerId).ToList();
            if (!workerAssignments.Any()) return false;

            var completionTime = DateTime.UtcNow;

            // 1. Завершаем работу текущего сотрудника (сразу по всем заказам в маршруте)
            await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId)
                .Set(a => a.Status, 2) // 2 = Completed
                .Set(a => a.CompletedAt, completionTime)
                .UpdateAsync();

            // 2. АВТО-ЗАКРЫТИЕ: Если кнопку нажал Главный, гасим Помощника
            if (workerAssignments.Any(a => a.Role == "Main"))
            {
                var helperAssignments = allAssignments.Where(a => a.Role == "Helper" && a.Status != 2).ToList();
                if (helperAssignments.Any())
                {
                    // Гасим сразу все строки помощника
                    await _db.GetTable<OrderHandoverAssignmentModel>()
                        .Where(a => a.TaskId == taskId && a.Role == "Helper")
                        .Set(a => a.Status, 2)
                        .Set(a => a.CompletedAt, completionTime)
                        .UpdateAsync();

                    _logger.LogInformation("Назначения помощника для пакетной задачи {TaskId} автоматически закрыты", taskId);
                }
            }

            return true;
        }
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

        public async Task ExecutePostCompletionLogicAsync(int taskId)
        {
            _logger.LogInformation("Запуск оптимизированной пост-обработки для задачи пакетной выдачи TaskId: {TaskId}", taskId);

            // 1. Пакетная загрузка всех назначений (заказов) в этой задаче
            var assignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId)
                .ToListAsync();

            if (!assignments.Any())
            {
                _logger.LogWarning("Назначения для задачи {TaskId} не найдены", taskId);
                return;
            }

            // 2. Пакетная загрузка ВСЕХ строк для ВСЕХ заказов одним запросом (как в старом коде)
            var assignmentIds = assignments.Select(a => a.Id).ToList();
            var allLines = await _db.GetTable<OrderHandoverLineModel>()
                .Where(l => assignmentIds.Contains(l.OrderHandoverAssignmentId) && l.ScannedQuantity > 0)
                .ToListAsync();

            if (!allLines.Any())
            {
                _logger.LogWarning("Нет отсканированных товаров для задачи {TaskId}. Списание не требуется.", taskId);
                return;
            }

            // Группируем строки по ID назначения в памяти, чтобы не делать запросы в цикле
            var linesLookup = allLines.ToLookup(l => l.OrderHandoverAssignmentId);

            // 3. Работа в транзакции для обеспечения целостности данных
            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                foreach (var assignment in assignments)
                {
                    // Извлекаем строки для конкретного заказа из нашего Lookup
                    var currentOrderLines = linesLookup[assignment.Id].ToList();

                    if (!currentOrderLines.Any())
                        continue; // Пропускаем, если по этому заказу ничего не отсканировали

                    // Проверка наличия исполнителя (безопасность из нового кода)
                    if (!assignment.AssignedToUserId.HasValue)
                    {
                        throw new InvalidOperationException($"Ошибка целостности: у назначения {assignment.Id} нет исполнителя!");
                    }

                    var workerId = assignment.AssignedToUserId.Value;

                    // Логика обработки в зависимости от типа выдачи
                    if (assignment.HandoverType == "ToCustomer")
                    {
                        await ProcessHandoverToCustomerAsync(assignment.OrderId, currentOrderLines, taskId, workerId);
                    }
                    else if (assignment.HandoverType == "ToCourier")
                    {
                        if (assignment.TargetCourierId == null)
                            throw new InvalidOperationException($"Не указан ID курьера для заказа {assignment.OrderId}!");

                        await ProcessHandoverToCourierAsync(assignment.OrderId, assignment.TargetCourierId.Value, currentOrderLines, taskId, workerId);
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Пост-обработка пакетной задачи {TaskId} успешно завершена", taskId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка транзакции при пост-обработке задачи выдачи {TaskId}", taskId);
                throw;
            }
        }

        private async Task ProcessHandoverToCustomerAsync(int orderId, List<OrderHandoverLineModel> lines, int taskId, int workerId)
        {
            var itemPositions = _db.GetTable<ItemPositionModel>();
            var reservations = _db.GetTable<OrderReservationModel>();
            var movements = _db.GetTable<ItemMovementModel>(); // Добавлено для логирования

            foreach (var line in lines)
            {
                if (line.ItemPositionId == null) continue;

                var sourceItemPos = await itemPositions.FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId.Value);
                if (sourceItemPos == null) continue;

                // А. СНАЧАЛА удаляем резерв конкретно нашего заказа
                await reservations
                    .Where(r => r.OrderPositionId == line.OrderPositionId)
                    .DeleteAsync();

                // Б. Считаем остаток
                var remainingQty = sourceItemPos.Quantity - line.ScannedQuantity;

                if (remainingQty <= 0)
                {
                    // WMS SELF-HEALING 1: Отвязываем чужие резервы
                    await reservations
                        .Where(r => r.ItemPositionId == sourceItemPos.Id)
                        .Set(r => r.ItemPositionId, (int?)null)
                        .UpdateAsync();

                    // WMS SELF-HEALING 2: Отвязываем строки задания выдачи (чтобы не нарушать FK)
                    await _db.GetTable<OrderHandoverLineModel>()
                        .Where(l => l.ItemPositionId == sourceItemPos.Id)
                        .Set(l => l.ItemPositionId, (int?)null)
                        .UpdateAsync();

                    // Теперь база разрешит удалить пустую ячейку
                    await itemPositions.Where(ip => ip.Id == sourceItemPos.Id).DeleteAsync();
                }
                else
                {
                    await itemPositions
                        .Where(ip => ip.Id == sourceItemPos.Id)
                        .Set(ip => ip.Quantity, remainingQty)
                        .UpdateAsync();
                }

                // В. Логируем перемещение
                await _db.InsertAsync(new ItemMovementModel
                {
                    ItemId = sourceItemPos.ItemId,
                    SourcePositionId = remainingQty <= 0 ? (int?)null : sourceItemPos.PositionId,
                    DestinationPositionId = null,
                    Quantity = line.ScannedQuantity,
                    TaskId = taskId,       // ИСПОЛЬЗУЕМ ЗДЕСЬ
                    WorkerId = workerId,   // И ЗДЕСЬ
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.GetTable<OrderModel>()
                .Where(o => o.OrderId == orderId)
                .Set(o => o.Status, "Completed")
                .UpdateAsync();
        }


        private async Task ProcessHandoverToCourierAsync(int orderId, int courierId, List<OrderHandoverLineModel> lines, int taskId, int workerId)
        {
            var courierPosition = await _db.GetTable<PositionModel>()
                .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == courierId.ToString());

            if (courierPosition == null)
                throw new InvalidOperationException($"Виртуальная ячейка для курьера ID {courierId} не найдена!");

            var itemPositions = _db.GetTable<ItemPositionModel>();
            var reservations = _db.GetTable<OrderReservationModel>();
            var movements = _db.GetTable<ItemMovementModel>();

            foreach (var line in lines)
            {
                if (line.ItemPositionId == null) continue;

                var sourceItemPos = await itemPositions.FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId.Value);
                if (sourceItemPos == null) continue;

                // А. Ищем или создаем товар в багажнике курьера
                var courierItemPos = await itemPositions
                    .FirstOrDefaultAsync(ip => ip.PositionId == courierPosition.PositionId && ip.ItemId == sourceItemPos.ItemId);

                int newCourierItemPosId;
                if (courierItemPos != null)
                {
                    await itemPositions
                        .Where(ip => ip.Id == courierItemPos.Id)
                        .Set(ip => ip.Quantity, ip => ip.Quantity + line.ScannedQuantity)
                        .UpdateAsync();
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

                // Б. Перепривязываем резерв нашего заказа в багажник
                await reservations
                    .Where(r => r.OrderPositionId == line.OrderPositionId)
                    .Set(r => r.ItemPositionId, newCourierItemPosId)
                    .UpdateAsync();

                // В. Считаем остаток на старой полке
                var remainingSourceQty = sourceItemPos.Quantity - line.ScannedQuantity;

                if (remainingSourceQty <= 0)
                {
                    // WMS SELF-HEALING 1: Очищаем старую пустую полку от резервов
                    await reservations
                        .Where(r => r.ItemPositionId == sourceItemPos.Id)
                        .Set(r => r.ItemPositionId, (int?)null)
                        .UpdateAsync();

                    // WMS SELF-HEALING 2: Отвязываем строки задания выдачи
                    await _db.GetTable<OrderHandoverLineModel>()
                        .Where(l => l.ItemPositionId == sourceItemPos.Id)
                        .Set(l => l.ItemPositionId, (int?)null)
                        .UpdateAsync();

                    // Удаляем пустую ячейку
                    await itemPositions.Where(ip => ip.Id == sourceItemPos.Id).DeleteAsync();
                }
                else
                {
                    await itemPositions
                        .Where(ip => ip.Id == sourceItemPos.Id)
                        .Set(ip => ip.Quantity, remainingSourceQty)
                        .UpdateAsync();
                }

                // Г. Логируем
                // Г. Логируем перемещение
                await _db.InsertAsync(new ItemMovementModel
                {
                    ItemId = sourceItemPos.ItemId, // <-- Пишем ID товара
                    SourcePositionId = sourceItemPos.PositionId, // <-- Складская полка
                    DestinationPositionId = courierPosition.PositionId, // <-- Полка багажника курьера
                    Quantity = line.ScannedQuantity,
                    TaskId = taskId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.GetTable<OrderModel>()
                .Where(o => o.OrderId == orderId)
                .Set(o => o.Status, "InTransit")
                .UpdateAsync();
        }

        public async Task<bool> AssignTaskToWorkerAsync(int taskId, int workerId)
        {
            // Проверяем, наша ли это задача вообще (чтобы не трогать инвентаризацию и сборку)
            var isOurTask = await _db.GetTable<BaseTaskModel>()
                                     .AnyAsync(t => t.TaskId == taskId && t.Type == this.TaskType);
            if (!isOurTask) return false;

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                var assignmentsToUpdate = await _db.GetTable<OrderHandoverAssignmentModel>()
                    .Where(a => a.TaskId == taskId && a.AssignedToUserId == null)
                    .ToListAsync();

                if (!assignmentsToUpdate.Any()) return false;

                foreach (var assignment in assignmentsToUpdate)
                {
                    await _db.GetTable<OrderHandoverAssignmentModel>()
                        .Where(a => a.Id == assignment.Id)
                        .Set(a => a.AssignedToUserId, workerId)
                        .UpdateAsync();
                }

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
                _logger.LogError(ex, "Ошибка присвоения задачи {TaskId} сотруднику {WorkerId}", taskId, workerId);
                return false;
            }
        }


    }
}