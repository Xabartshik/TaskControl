using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Providers
{
    public class ReturnExecutionProvider : ITaskExecutionProvider
    {
        private readonly ITaskDataConnection _db;
        private readonly IBaseTaskService _baseTaskService;
        private readonly ILogger<ReturnExecutionProvider> _logger;
        private readonly ITaskComplexityCalculator _complexityCalculator;
        private readonly TaskWorkloadAggregator _aggregator;

        public string TaskType => "ReturnToStock";

        public ReturnExecutionProvider(
            ITaskDataConnection db,
            IBaseTaskService baseTaskService,
            ILogger<ReturnExecutionProvider> logger,
            ITaskComplexityCalculator complexityCalculator,
            TaskWorkloadAggregator aggregator)
        {
            _db = db;
            _baseTaskService = baseTaskService;
            _logger = logger;
            _complexityCalculator = complexityCalculator;
            _aggregator = aggregator;
        }

        public async Task<bool> TryActivateTaskAsync(int taskId, int workerId)
        {
            await PauseActiveTasksAsync(workerId, taskId);

            var updated = await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId)
                .Set(a => a.Status, 1) // InProgress
                .Set(a => a.StartedAt, DateTime.UtcNow)
                .UpdateAsync();

            if (updated > 0)
            {
                await _db.GetTable<BaseTaskModel>()
                    .Where(t => t.TaskId == taskId && t.Status != "InProgress")
                    .Set(t => t.Status, "InProgress")
                    .UpdateAsync();
                return true;
            }
            return false;
        }

        public async Task<(bool Success, string Message)> ProcessScanAsync(int taskId, int workerId, string barcode)
        {
            var assignment = await _db.GetTable<ReturnAssignmentModel>()
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedToUserId == workerId && a.Status == 1);

            if (assignment == null) return (false, "Активная задача возврата не найдена.");

            // === НОВАЯ ЛОГИКА: ПЕРЕХВАТ СКАНИРОВАНИЯ ЯЧЕЙКИ ИЗ FLUTTER ===
            // Если мобилка прислала команду изменения ячейки (например: "CELL|45|STORAGE-RACK-01")
            if (barcode.StartsWith("CELL|"))
            {
                var parts = barcode.Split('|');
                if (parts.Length == 3 && int.TryParse(parts[1], out int lineId))
                {
                    string cellCode = parts[2].Trim();

                    var branchPositions = await _db.GetTable<PositionModel>()
                        .Where(p => p.BranchId == assignment.BranchId)
                        .ToListAsync();

                    var matchedPos = branchPositions.FirstOrDefault(p => GetFullPositionCode(p) == cellCode);

                    if (matchedPos != null)
                    {
                        // Мгновенно сохраняем новую целевую ячейку в базу!
                        await _db.GetTable<ReturnLineModel>()
                            .Where(l => l.Id == lineId)
                            .Set(l => l.TargetPositionId, matchedPos.PositionId)
                            .UpdateAsync();

                        return (true, $"Товар перенаправлен в ячейку:\n{cellCode}");
                    }
                    return (false, $"Ячейка {cellCode} не найдена в этом филиале.");
                }
            }

            // === СТАРАЯ ЛОГИКА: СКАНИРОВАНИЕ ШТРИХ-КОДА ТОВАРА ===
            var lines = await _db.GetTable<ReturnLineModel>()
                .Where(l => l.ReturnAssignmentId == assignment.Id)
                .ToListAsync();

            if (int.TryParse(barcode, out int itemId))
            {
                var itemPositions = _db.GetTable<ItemPositionModel>();
                ReturnLineModel targetLine = null;

                foreach (var line in lines)
                {
                    if (line.ScannedQuantity >= line.Quantity) continue;

                    var ip = await itemPositions.FirstOrDefaultAsync(x => x.Id == line.ItemPositionId);
                    if (ip != null && ip.ItemId == itemId)
                    {
                        targetLine = line;
                        break;
                    }
                }

                if (targetLine == null) return (false, "Товар не найден или уже собран.");

                await _db.GetTable<ReturnLineModel>()
                    .Where(l => l.Id == targetLine.Id)
                    .Set(l => l.ScannedQuantity, l => l.ScannedQuantity + 1)
                    .UpdateAsync();

                return (true, "Товар отсканирован.");
            }

            return (false, "Неизвестный штрих-код.");
        }
        public async Task<bool> TryCompleteAssignmentAsync(int taskId, int workerId, Dictionary<int, int>? cancelledLines = null)
        {
            _logger.LogInformation("[RETURN] Завершение возврата {TaskId} (исполнитель {WorkerId})", taskId, workerId);

            var isOurTask = await _db.GetTable<BaseTaskModel>()
                .AnyAsync(t => t.TaskId == taskId && t.Type == this.TaskType);
            if (!isOurTask) return false;

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                var currentAssignment = await _db.GetTable<ReturnAssignmentModel>()
                    .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedToUserId == workerId);

                if (currentAssignment == null) return false;

                var lines = await _db.GetTable<ReturnLineModel>()
                    .Where(l => l.ReturnAssignmentId == currentAssignment.Id)
                    .ToListAsync();

                foreach (var line in lines)
                {
                    // ПРОПУСКАЕМ товары, которые не отсканировали или не назначили ячейку
                    if (line.ScannedQuantity == 0 || !line.TargetPositionId.HasValue)
                    {
                        _logger.LogWarning("[RETURN] Линия {LineId} пропущена: не собрана или нет целевой ячейки.", line.Id);
                        continue;
                    }

                    int finalPosId = line.TargetPositionId.Value;
                    int qtyToMove = line.ScannedQuantity;

                    var sourceItemPos = await _db.GetTable<ItemPositionModel>().FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId);
                    if (sourceItemPos != null)
                    {
                        int sourceOriginalPosId = sourceItemPos.PositionId;

                        var existingTargetPos = await _db.GetTable<ItemPositionModel>()
                            .FirstOrDefaultAsync(ip => ip.PositionId == finalPosId && ip.ItemId == sourceItemPos.ItemId);

                        // 1. Складываем остатки (Консолидация) или создаем новые
                        if (existingTargetPos != null)
                        {
                            await _db.GetTable<ItemPositionModel>().Where(ip => ip.Id == existingTargetPos.Id)
                                .Set(ip => ip.Quantity, ip => ip.Quantity + qtyToMove).UpdateAsync();
                        }
                        else
                        {
                            await _db.InsertAsync(new ItemPositionModel
                            {
                                ItemId = sourceItemPos.ItemId,
                                PositionId = finalPosId,
                                Quantity = qtyToMove,
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        // 2. Вычитаем из источника
                        if (sourceItemPos.Quantity <= qtyToMove)
                            await _db.GetTable<ItemPositionModel>().Where(ip => ip.Id == sourceItemPos.Id).DeleteAsync();
                        else
                            await _db.GetTable<ItemPositionModel>().Where(ip => ip.Id == sourceItemPos.Id)
                                .Set(ip => ip.Quantity, ip => ip.Quantity - qtyToMove).UpdateAsync();

                        // 3. Логируем перемещение
                        await _db.InsertAsync(new ItemMovementModel
                        {
                            ItemId = sourceItemPos.ItemId,
                            SourcePositionId = sourceOriginalPosId,
                            DestinationPositionId = finalPosId,
                            Quantity = qtyToMove,
                            WorkerId = workerId,
                            CreatedAt = DateTime.UtcNow
                        });

                        // 4. Обновляем статусы ячеек
                        await UpdatePositionStatus(sourceOriginalPosId);
                        await _db.GetTable<PositionModel>().Where(p => p.PositionId == finalPosId)
                            .Set(p => p.Status, "Occupied").UpdateAsync();
                    }
                }

                var now = DateTime.UtcNow;
                if (currentAssignment.Role == "Main")
                {
                    await _db.GetTable<ReturnAssignmentModel>().Where(a => a.TaskId == taskId)
                        .Set(a => a.Status, 3).Set(a => a.CompletedAt, now).UpdateAsync();

                    await _db.GetTable<BaseTaskModel>().Where(t => t.TaskId == taskId)
                        .Set(t => t.Status, "Completed").UpdateAsync();
                }
                else
                {
                    await _db.GetTable<ReturnAssignmentModel>().Where(a => a.Id == currentAssignment.Id)
                        .Set(a => a.Status, 3).Set(a => a.CompletedAt, now).UpdateAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "[RETURN] Ошибка при завершении TaskId: {TaskId}", taskId);
                return false;
            }
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

        private async Task UpdatePositionStatus(int positionId)
        {
            var hasItems = await _db.GetTable<ItemPositionModel>().AnyAsync(ip => ip.PositionId == positionId && ip.Quantity > 0);
            if (!hasItems)
            {
                await _db.GetTable<PositionModel>()
                    .Where(p => p.PositionId == positionId)
                    .Set(p => p.Status, "Active")
                    .UpdateAsync();
            }
        }
        public async Task ExecutePostCompletionLogicAsync(int taskId)
        {
            _logger.LogInformation("Пост-обработка возврата TaskId: {TaskId}", taskId);

            var assignments = await _db.GetTable<ReturnAssignmentModel>().Where(a => a.TaskId == taskId).ToListAsync();
            if (!assignments.Any()) return;

            var assignmentIds = assignments.Select(a => a.Id).ToList();
            var allLines = await _db.GetTable<ReturnLineModel>()
                .Where(l => assignmentIds.Contains(l.ReturnAssignmentId) && l.ScannedQuantity > 0)
                .ToListAsync();

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                // Ищем любую дефолтную полку STORAGE на случай, если кладовщик забыл отсканировать полку
                var defaultStorage = await _db.GetTable<PositionModel>().FirstOrDefaultAsync(p => p.ZoneCode == "STORAGE")
                                     ?? await _db.GetTable<PositionModel>().FirstAsync();

                foreach (var line in allLines)
                {
                    var targetPosId = line.TargetPositionId ?? defaultStorage.PositionId;

                    var itemPos = await _db.GetTable<ItemPositionModel>().FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId);
                    if (itemPos != null)
                    {
                        var oldPosId = itemPos.PositionId;

                        // ВОТ ОНА - МАГИЯ ПЕРЕМЕЩЕНИЯ (Putaway)
                        await _db.GetTable<ItemPositionModel>()
                            .Where(ip => ip.Id == itemPos.Id)
                            .Set(ip => ip.PositionId, targetPosId)
                            .UpdateAsync();

                        // Логируем, что товар вернулся из багажника на склад
                        await _db.InsertAsync(new ItemMovementModel
                        {
                            ItemId = itemPos.ItemId,
                            SourcePositionId = oldPosId, // Виртуальная ячейка курьера
                            DestinationPositionId = targetPosId, // Полка на складе
                            Quantity = line.ScannedQuantity,
                            TaskId = taskId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _db.GetTable<BaseTaskModel>()
                        .Where(t => t.TaskId == taskId)
                        .Set(t => t.Status, "Completed")
                        .Set(t => t.CompletedAt, DateTime.UtcNow)
                        .UpdateAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при обработке возвратов");
                throw;
            }
        }

        // =========================================================================
        // МЕТОДЫ, КОТОРЫЕ НАМ НЕ НУЖНЫ, НО ИХ ТРЕБУЕТ ИНТЕРФЕЙС
        // =========================================================================

        public async Task<object?> GetTaskDetailsAsync(int taskId, int workerId)
        {
            // Здесь ты можешь собрать DTO для отображения на экране кладовщика
            // Пока возвращаем заглушку, чтобы код компилировался
            return new { TaskId = taskId, Message = "Задача на раскладку возвратов" };
        }

        public async Task<(bool Success, string Message)> TryCompleteWithCourierQrAsync(int taskId, int workerId, string qrToken)
            => (false, "Для возвратов это действие недоступно");

        public async Task<bool> TryCompleteAssignmentAsync(int taskId, int workerId)
            => await TryCompleteAssignmentAsync(taskId, workerId, null);



        public async Task<bool> TryPauseTaskAsync(int taskId, int workerId)
        {
            return await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId)
                .Set(a => a.Status, 2)
                .UpdateAsync() > 0;
        }

        public async Task<bool> TryCancelTaskAsync(int taskId, int workerId)
        {
            return await _db.GetTable<ReturnAssignmentModel>()
               .Where(a => a.TaskId == taskId && a.AssignedToUserId == workerId)
               .Set(a => a.Status, 3)
               .UpdateAsync() > 0;
        }

        public async Task<bool> IsTaskFullyCompletedAsync(int taskId)
        {
            var statuses = await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.TaskId == taskId).Select(a => a.Status).ToListAsync();
            return statuses.All(s => s == 2 || s == 3);
        }

        public async Task PauseActiveTasksAsync(int workerId, int excludeTaskId)
        {
            await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.AssignedToUserId == workerId && a.Status == 1 && a.TaskId != excludeTaskId)
                .Set(a => a.Status, 0)
                .UpdateAsync();
        }

        public async Task<bool> AssignTaskToWorkerAsync(int taskId, int workerId)
        {
            var isOurTask = await _db.GetTable<BaseTaskModel>()
                .AnyAsync(t => t.TaskId == taskId && t.Type == this.TaskType);
            if (!isOurTask) return false;

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                // 1. Находим все неназначенные роли для этой задачи
                var assignments = await _db.GetTable<ReturnAssignmentModel>()
                    .Where(a => a.TaskId == taskId && a.AssignedToUserId == null)
                    .ToListAsync();

                if (!assignments.Any()) return false;

                // 2. Назначаем текущего сотрудника на роль "Main"
                var mainAssignment = assignments.FirstOrDefault(a => a.Role == "Main");
                if (mainAssignment == null) return false; // Кто-то уже перехватил роль

                await _db.GetTable<ReturnAssignmentModel>()
                    .Where(a => a.Id == mainAssignment.Id)
                    .Set(a => a.AssignedToUserId, workerId)
                    .Set(a => a.Status, 0) // Статус Assigned
                    .UpdateAsync();

                // 3. АВТОПОИСК ПОМОЩНИКА (Срабатывает только когда взят Main)
                var helperAssignment = assignments.FirstOrDefault(a => a.Role == "Helper");
                if (helperAssignment != null)
                {
                    // Ищем свободного человека
                    int? autoHelperId = await _aggregator.FindAvailableHelperAsync(mainAssignment.BranchId, workerId);

                    if (autoHelperId.HasValue)
                    {
                        await _db.GetTable<ReturnAssignmentModel>()
                            .Where(a => a.Id == helperAssignment.Id)
                            .Set(a => a.AssignedToUserId, autoHelperId.Value)
                            .UpdateAsync();

                        _logger.LogInformation("Для задачи возврата {TaskId} автоматически назначен помощник {HelperId}", taskId, autoHelperId);
                    }
                    else
                    {
                        _logger.LogInformation("Свободных помощников для возврата {TaskId} не найдено. Вакансия остается в пуле.", taskId);
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
                _logger.LogError(ex, "Ошибка при динамическом назначении помощника для возврата {TaskId}", taskId);
                return false;
            }
        }
    }
}