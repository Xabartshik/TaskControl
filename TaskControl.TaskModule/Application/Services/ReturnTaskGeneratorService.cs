using LinqToDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services
{
    public class ReturnTaskGeneratorService
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<ReturnTaskGeneratorService> _logger;
        private readonly ITaskComplexityCalculator _complexityCalculator;
        private readonly TaskWorkloadAggregator _aggregator;

        public ReturnTaskGeneratorService(
            ITaskDataConnection db,
            ILogger<ReturnTaskGeneratorService> logger,
            ITaskComplexityCalculator complexityCalculator,
            TaskWorkloadAggregator aggregator)
        {
            _db = db;
            _logger = logger;
            _complexityCalculator = complexityCalculator;
            _aggregator = aggregator;
        }

        /// <summary>
        /// Сценарий 1: Генерация возврата из отмененных позиций заказа (Вызывается из OrderHandover)
        /// </summary>
        public async Task<int?> GenerateReturnTaskFromCancelledItemsAsync(int orderId, int branchId, List<(int ItemPositionId, int Qty)> itemsToReturn)
        {
            if (!itemsToReturn.Any()) return null;
            _logger.LogInformation("Генерация задачи возврата для отмененных товаров заказа #{OrderId}", orderId);
            string titleBase = $"Возврат отмененных товаров (Заказ #{orderId})";
            return await GenerateReturnTaskCoreAsync(titleBase, branchId, itemsToReturn);
        }

        /// <summary>
        /// Сценарий 2: Генерация возврата "отказников" из багажника курьера (Вызывается из Observer'а)
        /// </summary>
        public async Task<int?> GenerateReturnTaskForCourierAsync(int courierId, int branchId, List<(int ItemPositionId, int Qty)> itemsToReturn)
        {
            if (!itemsToReturn.Any()) return null;
            _logger.LogInformation("Генерация задачи возврата из багажника курьера #{CourierId}", courierId);
            string titleBase = $"Возврат на склад (Курьер #{courierId})";
            return await GenerateReturnTaskCoreAsync(titleBase, branchId, itemsToReturn);
        }

        /// <summary>
        /// Универсальное ядро генерации задачи возврата с автоматическим определением 
        /// точек забора (Source) и размещения (Target), а также расчетом сложности.
        /// </summary>
        private async Task<int?> GenerateReturnTaskCoreAsync(string titleBase, int branchId, List<(int ItemPositionId, int Qty)> itemsToReturn)
        {
            var returnMetrics = new List<TaskItemMetrics>();
            // Временный список, чтобы не запрашивать ItemModel и ItemPositionModel повторно в циклах
            var itemsData = new List<(int ItemPositionId, int Qty, ItemModel Model, int? SourcePosId)>();

            foreach (var item in itemsToReturn)
            {
                var itemPos = await _db.GetTable<ItemPositionModel>().FirstOrDefaultAsync(ip => ip.Id == item.ItemPositionId);
                if (itemPos == null) continue;

                var itemModel = await _db.GetTable<ItemModel>().FirstOrDefaultAsync(i => i.ItemId == itemPos.ItemId);
                if (itemModel == null) continue;

                // === ЛОГИКА ОПРЕДЕЛЕНИЯ ТОЧКИ ЗАБОРА (Source) ===
                // Проверяем, было ли движение этого товара недавно (например, в зону EXPRESS при сборке).
                // Это позволит кладовщику точно знать, что товар нужно забрать из зоны выдачи, а не искать его на основной полке.
                var lastMovement = await _db.GetTable<ItemMovementModel>()
                    .Where(m => m.ItemId == itemModel.ItemId && m.Quantity >= item.Qty)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();

                // Если движения не было, считаем точкой забора текущую позицию записи ItemPosition
                int? sourcePosId = lastMovement?.DestinationPositionId ?? itemPos.PositionId;

                returnMetrics.Add(new TaskItemMetrics
                {
                    WeightGrams = itemModel.Weight,
                    LengthMm = itemModel.Length,
                    WidthMm = itemModel.Width,
                    HeightMm = itemModel.Height,
                    Quantity = item.Qty
                });

                itemsData.Add((item.ItemPositionId, item.Qty, itemModel, sourcePosId));
            }

            if (!itemsData.Any()) return null;

            // 1. Расчет сложности и определение необходимости напарника
            var returnComplexity = _complexityCalculator.CalculateForItems(returnMetrics);
            int priority = returnComplexity.RequiresHelper ? (int)TaskPriority.High : (int)TaskPriority.Background;
            string finalTitle = returnComplexity.RequiresHelper ? $"[ТЯЖЕЛЫЙ] {titleBase}" : titleBase;

            // 2. Создание базовой задачи
            int returnTaskId = Convert.ToInt32(await _db.InsertWithIdentityAsync(new BaseTaskModel
            {
                Title = finalTitle,
                Type = "ReturnToStock",
                Status = "New",
                PriorityLevel = priority,
                BranchId = branchId,
                CreatedAt = DateTime.UtcNow
            }));

            // 3. Создание назначения для основного сотрудника (Main)
            int returnAssignmentId = Convert.ToInt32(await _db.InsertWithIdentityAsync(new ReturnAssignmentModel
            {
                TaskId = returnTaskId,
                BranchId = branchId,
                Status = 0,
                Role = "Main",
                Complexity = returnComplexity.MainComplexity,
                AssignedAt = DateTime.UtcNow
            }));

            // 4. Если задача тяжелая — создаем вакансию для помощника (Helper)
            if (returnComplexity.RequiresHelper)
            {
                await _db.InsertAsync(new ReturnAssignmentModel
                {
                    TaskId = returnTaskId,
                    BranchId = branchId,
                    Status = 0,
                    Role = "Helper",
                    AssignedToUserId = null, // Оставляем пустым для свободного пула
                    Complexity = returnComplexity.HelperComplexity,
                    AssignedAt = DateTime.UtcNow
                });
            }

            // 5. Формирование строк задачи с указанием маршрута (Откуда -> Куда)
            foreach (var data in itemsData)
            {
                // Алгоритм подбора оптимальной ячейки для возврата (Target)
                int? suggestedTargetPosId = await SuggestTargetPositionAsync(data.Model, branchId);

                await _db.InsertAsync(new ReturnLineModel
                {
                    ReturnAssignmentId = returnAssignmentId,

                    // ИСПРАВЛЕНИЕ: Пишем настоящий ID позиции товара, а не номер полки
                    ItemPositionId = data.ItemPositionId,

                    Quantity = data.Qty,
                    ScannedQuantity = 0,
                    TargetPositionId = suggestedTargetPosId
                });
            }

            return returnTaskId;
        }
        private async Task<int?> SuggestTargetPositionAsync(ItemModel item, int branchId)
        {
            var reservedZones = new[] { "PICKUP", "BULK", "COURIER", "EXPRESS" };

            // 1. Консолидация (ищем, где уже лежит такой товар)
            var existingPos = await (from p in _db.GetTable<PositionModel>()
                                     join ip in _db.GetTable<ItemPositionModel>() on p.PositionId equals ip.PositionId
                                     where p.BranchId == branchId && !reservedZones.Contains(p.ZoneCode) && ip.ItemId == item.ItemId
                                     select p.PositionId).FirstOrDefaultAsync();

            if (existingPos != 0) return existingPos;

            // 2. Поиск пустой полки по габаритам
            var emptyFittedPos = await (from p in _db.GetTable<PositionModel>()
                                        where p.BranchId == branchId && !reservedZones.Contains(p.ZoneCode) && p.Status == "Active"
                                           && !_db.GetTable<ItemPositionModel>().Any(ip => ip.PositionId == p.PositionId)
                                           && (p.Length >= item.Length && p.Width >= item.Width && p.Height >= item.Height)
                                        select p.PositionId).FirstOrDefaultAsync();

            if (emptyFittedPos != 0) return emptyFittedPos;

            // 3. Запасной вариант (любая пустая активная ячейка)
            var fallbackPos = await (from p in _db.GetTable<PositionModel>()
                                     where p.BranchId == branchId && !reservedZones.Contains(p.ZoneCode) && p.Status == "Active"
                                        && !_db.GetTable<ItemPositionModel>().Any(ip => ip.PositionId == p.PositionId)
                                     select p.PositionId).FirstOrDefaultAsync();

            return fallbackPos != 0 ? fallbackPos : null;
        }
    }
}