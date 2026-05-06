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
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.Application.Providers
{
    public class ReturnExecutionProvider : ITaskExecutionProvider
    {
        private readonly ITaskDataConnection _db;
        private readonly IBaseTaskService _baseTaskService;
        private readonly ILogger<ReturnExecutionProvider> _logger;

        // Провайдер реагирует только на этот тип задач
        public string TaskType => "ReturnToStock";

        public ReturnExecutionProvider(
            ITaskDataConnection db,
            IBaseTaskService baseTaskService,
            ILogger<ReturnExecutionProvider> logger)
        {
            _db = db;
            _baseTaskService = baseTaskService;
            _logger = logger;
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

            var lines = await _db.GetTable<ReturnLineModel>()
                .Where(l => l.ReturnAssignmentId == assignment.Id)
                .ToListAsync();

            var itemPositions = _db.GetTable<ItemPositionModel>();
            var positions = _db.GetTable<PositionModel>();

            // 1. Пытаемся понять, это сканирование полки или товара?
            // Допустим, штрих-код полки - это её FLSNumber или PositionId
            var scannedPosition = await positions.FirstOrDefaultAsync(p => p.FLSNumber == barcode || p.PositionId.ToString() == barcode);

            if (scannedPosition != null)
            {
                // Это полка! Привязываем её ко всем отсканированным товарам, у которых еще нет целевой полки
                var linesToUpdate = lines.Where(l => l.ScannedQuantity > 0 && l.TargetPositionId == null).ToList();
                if (!linesToUpdate.Any()) return (false, "Нет отсканированных товаров для размещения на эту полку.");

                foreach (var line in linesToUpdate)
                {
                    await _db.GetTable<ReturnLineModel>()
                        .Where(l => l.Id == line.Id)
                        .Set(l => l.TargetPositionId, scannedPosition.PositionId)
                        .UpdateAsync();
                }
                return (true, $"Полка {scannedPosition.ZoneCode}-{scannedPosition.FLSNumber} привязана к товарам.");
            }

            // 2. Если это не полка, значит это товар
            if (int.TryParse(barcode, out int itemId))
            {
                ReturnLineModel targetLine = null;
                foreach (var line in lines)
                {
                    if (line.ScannedQuantity >= line.Quantity) continue; // Этот уже взяли

                    var ip = await itemPositions.FirstOrDefaultAsync(x => x.Id == line.ItemPositionId);
                    if (ip != null && ip.ItemId == itemId)
                    {
                        targetLine = line;
                        break;
                    }
                }

                if (targetLine == null) return (false, "Товар не из этого списка возврата или уже собран.");

                await _db.GetTable<ReturnLineModel>()
                    .Where(l => l.Id == targetLine.Id)
                    .Set(l => l.ScannedQuantity, l => l.ScannedQuantity + 1)
                    .UpdateAsync();

                return (true, "Товар отсканирован. Теперь отсканируйте полку хранения, куда вы его кладете.");
            }

            return (false, "Неизвестный штрих-код.");
        }

        public async Task<bool> TryCompleteAssignmentAsync(int taskId, int workerId, Dictionary<int, int>? cancelledLines = null)
        {
            var assignment = await _db.GetTable<ReturnAssignmentModel>()
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedToUserId == workerId);

            if (assignment == null) return false;

            await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.Id == assignment.Id)
                .Set(a => a.Status, 2) // Completed
                .Set(a => a.CompletedAt, DateTime.UtcNow)
                .UpdateAsync();

            return true;
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
                .Set(a => a.Status, 0)
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
            var updated = await _db.GetTable<ReturnAssignmentModel>()
                   .Where(a => a.TaskId == taskId && a.AssignedToUserId == null)
                   .Set(a => a.AssignedToUserId, workerId)
                   .UpdateAsync();
            return updated > 0;
        }
    }
}