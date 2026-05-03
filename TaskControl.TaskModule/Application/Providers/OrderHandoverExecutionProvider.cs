using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model; // Для EmployeeModel
using TaskControl.InventoryModule.DataAccess.Model;   // Для PositionModel, ItemPositionModel, ItemModel
using TaskControl.OrderModule.DataAccess.Model;       // Для OrderModel
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
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

        public OrderHandoverExecutionProvider(
            ITaskDataConnection db,
            IBaseTaskService baseTaskService,
            ILogger<OrderHandoverExecutionProvider> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _baseTaskService = baseTaskService ?? throw new ArgumentNullException(nameof(baseTaskService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ==========================================
        // 1. ИНФОРМАЦИЯ ДЛЯ МОБИЛЬНОГО ПРИЛОЖЕНИЯ
        // ==========================================

        public async Task<object?> GetTaskDetailsAsync(int taskId, int workerId)
        {
            _logger.LogInformation("Получение деталей задачи выдачи TaskId: {TaskId} для WorkerId: {WorkerId}", taskId, workerId);

            // 1. Находим все назначения для этой задачи
            var allAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                                          .Where(a => a.TaskId == taskId)
                                          .ToListAsync();

            var currentAssignment = allAssignments.FirstOrDefault(a => a.AssignedToUserId == workerId);
            if (currentAssignment == null)
            {
                _logger.LogWarning("Назначение на выдачу не найдено для TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
                return null;
            }

            var baseTask = await _baseTaskService.GetById(taskId);
            var order = await _db.GetTable<OrderModel>().FirstOrDefaultAsync(o => o.OrderId == currentAssignment.OrderId);

            // 2. Логика кооператива (есть ли напарник-грузчик)
            var partnerAssignment = allAssignments.FirstOrDefault(a => a.Id != currentAssignment.Id);
            bool isCooperative = partnerAssignment != null;
            string partnerName = null;

            if (isCooperative)
            {
                var partnerUser = await _db.GetTable<EmployeeModel>().FirstOrDefaultAsync(u => u.EmployeesId == partnerAssignment.AssignedToUserId);
                partnerName = partnerUser?.FullName ?? $"Сотрудник ID: {partnerAssignment.AssignedToUserId}";
            }

            // 3. Собираем базовый DTO
            var dto = new HandoverTaskDetailsDto
            {
                AssignmentId = currentAssignment.Id,
                TaskId = currentAssignment.TaskId,
                TaskNumber = baseTask?.Title ?? $"Выдача заказа #{currentAssignment.OrderId}",
                OrderId = currentAssignment.OrderId,
                HandoverType = currentAssignment.HandoverType, // "ToCustomer" или "ToCourier"
                Status = currentAssignment.Status,
                IsCooperative = isCooperative,
                PartnerName = partnerName,
                PartnerStatus = partnerAssignment?.Status
            };

            // Добавляем инфу о курьере, если это передача курьеру
            if (currentAssignment.HandoverType == "ToCourier" && currentAssignment.TargetCourierId.HasValue)
            {
                var courierInfo = await _db.GetTable<EmployeeModel>()
                    .FirstOrDefaultAsync(u => u.EmployeesId == currentAssignment.TargetCourierId.Value);
                // Можно добавить поле TargetName в DTO, чтобы кассир/грузчик знал, кому отдает:
                dto.TargetName = courierInfo?.FullName ?? $"Курьер ID: {currentAssignment.TargetCourierId.Value}";
            }
            else
            {
                dto.TargetName = "Покупатель (Самовывоз)";
            }

            // 4. Собираем товары для выдачи
            var lines = await _db.GetTable<OrderHandoverLineModel>()
                                 .Where(l => l.OrderHandoverAssignmentId == currentAssignment.Id)
                                 .ToListAsync();

            var positions = _db.GetTable<PositionModel>();
            var itemPositions = _db.GetTable<ItemPositionModel>();
            var items = _db.GetTable<ItemModel>();

            foreach (var line in lines)
            {
                // Ищем инфу о товаре и ячейке, где он сейчас лежит
                var itemInfoQuery = from ip in itemPositions
                                    join i in items on ip.ItemId equals i.ItemId
                                    where ip.Id == line.ItemPositionId
                                    select new { i.ItemId, i.Name, ip.PositionId };

                var itemInfo = await itemInfoQuery.FirstOrDefaultAsync();

                // Формируем строковый код ячейки (чтобы грузчик знал, откуда забрать)
                string sourceCellCode = "Неизвестная ячейка";
                if (itemInfo != null)
                {
                    var sourcePosModel = await positions.FirstOrDefaultAsync(p => p.PositionId == itemInfo.PositionId);
                    sourceCellCode = GetFullPositionCode(sourcePosModel) ?? itemInfo.PositionId.ToString();
                }

                // Достаем оригинальную строчку заказа, чтобы получить цену (опционально)
                var orderPos = await _db.GetTable<OrderPositionModel>()
                    .FirstOrDefaultAsync(op => op.UniqueId == line.OrderPositionId);

                dto.ItemsToScan.Add(new HandoverItemDto
                {
                    LineId = line.Id,
                    ItemId = itemInfo?.ItemId ?? 0,
                    ItemName = itemInfo?.Name ?? "Неизвестный товар",
                    Barcode = (itemInfo?.ItemId ?? 0).ToString(), // В реальной жизни здесь штрихкод товара
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
            var assignment = await _db.GetTable<OrderHandoverAssignmentModel>()
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedToUserId == workerId && a.Status == 1);

            if (assignment == null)
                return (false, "Активная задача выдачи не найдена или не в статусе InProgress.");

            // В реальной системе здесь будет поиск ItemId по таблице штрих-кодов.
            // Пока используем допущение, что Barcode совпадает с ItemId (из GetTaskDetailsAsync)
            if (!int.TryParse(barcode, out int itemId))
                return (false, "Неверный формат штрих-кода.");

            // 2. Ищем строки задания
            var lines = await _db.GetTable<OrderHandoverLineModel>()
                .Where(l => l.OrderHandoverAssignmentId == assignment.Id)
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

            // 1. Ставим на паузу все другие активные задачи этого сотрудника
            await PauseActiveTasksAsync(workerId, taskId);

            // 2. Переводим текущее назначение в InProgress (1)
            var updated = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId)
                .Set(a => a.Status, 1) // 1 = InProgress
                .Set(a => a.StartedAt, DateTime.UtcNow)
                .UpdateAsync();

            if (updated > 0)
            {
                // Опционально: 
                /*
                await _db.GetTable<BaseTaskModel>()
                    .Where(t => t.TaskId == taskId && t.Status != "InProgress")
                    .Set(t => t.Status, "InProgress")
                    .UpdateAsync();
                */
                return true;
            }

            return false;
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

        public async Task<bool> TryCompleteAssignmentAsync(int taskId, int workerId)
        {
            _logger.LogInformation("Сотрудник {WorkerId} завершил свою часть выдачи {TaskId}", taskId, workerId);

            var updated = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId)
                .Set(a => a.Status, 2) // 2 = Completed
                .Set(a => a.CompletedAt, DateTime.UtcNow)
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
            _logger.LogInformation("Запуск пост-обработки для задачи выдачи TaskId: {TaskId}", taskId);

            // 1. Получаем базовую информацию о выдаче
            var assignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId)
                .ToListAsync();

            var mainAssignment = assignments.FirstOrDefault();
            if (mainAssignment == null) return;

            var orderId = mainAssignment.OrderId;
            var handoverType = mainAssignment.HandoverType;

            // 2. Достаем все строки (товары), которые были физически отсканированы
            var assignmentIds = assignments.Select(a => a.Id).ToList();
            var lines = await _db.GetTable<OrderHandoverLineModel>()
                .Where(l => assignmentIds.Contains(l.OrderHandoverAssignmentId) && l.ScannedQuantity > 0)
                .ToListAsync();

            if (!lines.Any())
            {
                _logger.LogWarning("Нет отсканированных товаров для задачи {TaskId}. Списание не требуется.", taskId);
                return;
            }

            // 3. Начинаем транзакцию, так как меняем баланс склада
            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                if (handoverType == "ToCustomer")
                {
                    await ProcessHandoverToCustomerAsync(orderId, lines);
                }
                else if (handoverType == "ToCourier")
                {
                    if (mainAssignment.TargetCourierId == null)
                        throw new InvalidOperationException("Не указан ID курьера для передачи!");

                    await ProcessHandoverToCourierAsync(orderId, mainAssignment.TargetCourierId.Value, lines);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Пост-обработка задачи выдачи {TaskId} успешно завершена", taskId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка транзакции при пост-обработке задачи выдачи {TaskId}", taskId);
                throw;
            }
        }

        private async Task ProcessHandoverToCustomerAsync(int orderId, List<OrderHandoverLineModel> lines)
        {
            var itemPositions = _db.GetTable<ItemPositionModel>();
            var movements = _db.GetTable<ItemMovementModel>();
            var reservations = _db.GetTable<OrderReservationModel>();

            foreach (var line in lines)
            {
                if (line.ItemPositionId == null) continue;

                var sourceItemPos = await itemPositions.FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId.Value);
                if (sourceItemPos == null) continue;

                // А. Списываем товар с полки выдачи
                await itemPositions
                    .Where(ip => ip.Id == sourceItemPos.Id)
                    .Set(ip => ip.Quantity, ip => ip.Quantity - line.ScannedQuantity)
                    .UpdateAsync();

                // Б. Логируем перемещение. DestinationPositionId = null, так как товар покинул склад и ушел клиенту
                await _db.InsertAsync(new ItemMovementModel
                {
                    SourceItemPositionId = sourceItemPos.Id,
                    DestinationPositionId = null,
                    Quantity = line.ScannedQuantity,
                    CreatedAt = DateTime.UtcNow
                });

                // В. Отвязываем резерв (товар физически отдан, резерв на складе больше не нужен)
                await reservations
                    .Where(r => r.OrderPositionId == line.OrderPositionId)
                    .Set(r => r.ItemPositionId, (int?)null)
                    .UpdateAsync();
            }

            // Обновляем статус заказа на "Завершен"
            await _db.GetTable<OrderModel>()
                .Where(o => o.OrderId == orderId)
                .Set(o => o.Status, "Completed")
                .UpdateAsync();
        }

        private async Task ProcessHandoverToCourierAsync(int orderId, int courierId, List<OrderHandoverLineModel> lines)
        {
            // 1. Ищем машину курьера (виртуальную ячейку, которую мы создали через Event)
            var courierPosition = await _db.GetTable<PositionModel>()
                .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == courierId.ToString());

            if (courierPosition == null)
                throw new InvalidOperationException($"Виртуальная ячейка для курьера ID {courierId} не найдена!");

            var itemPositions = _db.GetTable<ItemPositionModel>();
            var movements = _db.GetTable<ItemMovementModel>();
            var reservations = _db.GetTable<OrderReservationModel>();

            foreach (var line in lines)
            {
                if (line.ItemPositionId == null) continue;

                var sourceItemPos = await itemPositions.FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId.Value);
                if (sourceItemPos == null) continue;

                // А. Списываем со старой ячейки
                await itemPositions
                    .Where(ip => ip.Id == sourceItemPos.Id)
                    .Set(ip => ip.Quantity, ip => ip.Quantity - line.ScannedQuantity)
                    .UpdateAsync();

                // Б. Ищем, есть ли уже этот товар в багажнике курьера. Если есть - плюсуем, если нет - создаем
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

                // В. Логируем перемещение: Полка склада -> Багажник курьера
                await _db.InsertAsync(new ItemMovementModel
                {
                    SourceItemPositionId = sourceItemPos.Id,
                    DestinationPositionId = courierPosition.PositionId,
                    Quantity = line.ScannedQuantity,
                    CreatedAt = DateTime.UtcNow
                });

                // Г. САМОЕ ГЛАВНОЕ: Перепривязываем резерв! Товар все еще зарезервирован под клиента, но едет в машине
                await reservations
                    .Where(r => r.OrderPositionId == line.OrderPositionId)
                    .Set(r => r.ItemPositionId, newCourierItemPosId)
                    .UpdateAsync();
            }

            // Обновляем статус заказа на "В пути"
            await _db.GetTable<OrderModel>()
                .Where(o => o.OrderId == orderId)
                .Set(o => o.Status, "InTransit")
                .UpdateAsync();
        }
    }
}