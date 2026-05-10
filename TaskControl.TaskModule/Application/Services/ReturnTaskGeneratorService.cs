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
        /// Универсальное ядро генерации задачи возврата с авто-поиском помощника
        /// </summary>
        private async Task<int?> GenerateReturnTaskCoreAsync(string titleBase, int branchId, List<(int ItemPositionId, int Qty)> itemsToReturn)
        {
            var returnMetrics = new List<TaskItemMetrics>();

            foreach (var item in itemsToReturn)
            {
                var itemPos = await _db.GetTable<ItemPositionModel>().FirstOrDefaultAsync(ip => ip.Id == item.ItemPositionId);
                if (itemPos != null)
                {
                    var itemModel = await _db.GetTable<ItemModel>().FirstOrDefaultAsync(i => i.ItemId == itemPos.ItemId);
                    if (itemModel != null)
                    {
                        returnMetrics.Add(new TaskItemMetrics
                        {
                            WeightGrams = itemModel.Weight,
                            LengthMm = itemModel.Length,
                            WidthMm = itemModel.Width,
                            HeightMm = itemModel.Height,
                            Quantity = item.Qty
                        });
                    }
                }
            }

            var returnComplexity = _complexityCalculator.CalculateForItems(returnMetrics);
            int priority = returnComplexity.RequiresHelper ? (int)TaskPriority.High : (int)TaskPriority.Background;
            string finalTitle = returnComplexity.RequiresHelper ? $"[ТЯЖЕЛЫЙ] {titleBase}" : titleBase;

            int returnTaskId = Convert.ToInt32(await _db.InsertWithIdentityAsync(new BaseTaskModel
            {
                Title = finalTitle,
                Type = "ReturnToStock",
                Status = "New",
                PriorityLevel = priority,
                BranchId = branchId,
                CreatedAt = DateTime.UtcNow
            }));

            // Создаем Main назначение
            int returnAssignmentId = Convert.ToInt32(await _db.InsertWithIdentityAsync(new ReturnAssignmentModel
            {
                TaskId = returnTaskId,
                BranchId = branchId,
                Status = 0,
                Role = "Main",
                Complexity = returnComplexity.MainComplexity,
                AssignedAt = DateTime.UtcNow
            }));
            if (returnComplexity.RequiresHelper)
            {
                // Просто создаем вакансию помощника. Агрегатор здесь больше не вызываем!
                await _db.InsertAsync(new ReturnAssignmentModel
                {
                    TaskId = returnTaskId,
                    BranchId = branchId,
                    Status = 0, // New
                    Role = "Helper",
                    AssignedToUserId = null, // ВАЖНО: Оставляем пустым для пула
                    Complexity = returnComplexity.HelperComplexity,
                    AssignedAt = DateTime.UtcNow
                });
            }
            foreach (var item in itemsToReturn)
            {
                await _db.InsertAsync(new ReturnLineModel
                {
                    ReturnAssignmentId = returnAssignmentId,
                    ItemPositionId = item.ItemPositionId,
                    Quantity = item.Qty,
                    ScannedQuantity = 0
                });
            }

            return returnTaskId;
        }
    }
}