using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.Application.Observers
{
    public class ReturnTaskGeneratorObserver : IEmployeeCheckInObserver
    {
        private readonly ITaskDataConnection _taskDb;
        private readonly ILogger<ReturnTaskGeneratorObserver> _logger;

        public ReturnTaskGeneratorObserver(ITaskDataConnection taskDb, ILogger<ReturnTaskGeneratorObserver> logger)
        {
            _taskDb = taskDb;
            _logger = logger;
        }

        public async Task OnEmployeeCheckedAsync(int employeeId, int branchId, string checkType)
        {
            // Нас интересует только конец смены или прибытие на пандус
            if (checkType != "out" && checkType != "dock") return;

            var db = (DataConnection)_taskDb;

            // 1. Ищем виртуальную ячейку курьера
            var courierPos = await db.GetTable<PositionModel>()
                .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == employeeId.ToString());

            if (courierPos == null) return; // Это не курьер или у него нет машины

            // 2. Достаем все товары из багажника
            var itemsInTrunk = await db.GetTable<ItemPositionModel>()
                .Where(ip => ip.PositionId == courierPos.PositionId)
                .ToListAsync();

            if (!itemsInTrunk.Any()) return; // Багажник пуст

            // 3. Ищем резервы, чтобы отсеять отказников
            var itemPosIds = itemsInTrunk.Select(i => i.Id).ToList();
            var reservations = await db.GetTable<OrderReservationModel>()
                .Where(r => r.ItemPositionId.HasValue && itemPosIds.Contains(r.ItemPositionId.Value))
                .ToListAsync();

            var reservedIds = reservations.Select(r => r.ItemPositionId!.Value).ToHashSet();
            var orphanedItems = itemsInTrunk.Where(ip => !reservedIds.Contains(ip.Id)).ToList();

            if (!orphanedItems.Any()) return;

            // 4. Считаем вес
            var itemIds = orphanedItems.Select(o => o.ItemId).Distinct().ToList();
            var itemsInfo = await db.GetTable<ItemModel>().Where(i => itemIds.Contains(i.ItemId)).ToListAsync();

            double totalWeight = 0;
            foreach (var orphan in orphanedItems)
            {
                var info = itemsInfo.FirstOrDefault(i => i.ItemId == orphan.ItemId);
                if (info != null) totalWeight += (info.Weight * orphan.Quantity);
            }

            // 5. Генерируем задачи возврата (Transaction)
            using var tx = await db.BeginTransactionAsync();
            try
            {
                int taskId = await db.InsertWithInt32IdentityAsync(new BaseTaskModel
                {
                    Title = $"Возврат на склад (Курьер #{employeeId})",
                    Type = "ReturnToStock",
                    BranchId = branchId,
                    PriorityLevel = 2,
                    Status = "New",
                    CreatedAt = DateTime.UtcNow
                });

                int mainAssignmentId = await db.InsertWithInt32IdentityAsync(new ReturnAssignmentModel
                {
                    TaskId = taskId,
                    BranchId = branchId,
                    Status = 0,
                    Role = "Main",
                    Complexity = orphanedItems.Count * 0.5,
                    AssignedAt = DateTime.UtcNow
                });

                if (totalWeight >= 50.0)
                {
                    await db.InsertAsync(new ReturnAssignmentModel
                    {
                        TaskId = taskId,
                        BranchId = branchId,
                        Status = 0,
                        Role = "Helper",
                        Complexity = 0,
                        AssignedAt = DateTime.UtcNow
                    });
                }

                foreach (var orphan in orphanedItems)
                {
                    await db.InsertAsync(new ReturnLineModel
                    {
                        ReturnAssignmentId = mainAssignmentId,
                        ItemPositionId = orphan.Id,
                        Quantity = orphan.Quantity,
                        ScannedQuantity = 0
                    });
                }

                await tx.CommitAsync();
                _logger.LogInformation("✅ Сгенерирована задача ReturnToStock #{TaskId} для возвратов курьера {CourierId}. Вес: {Weight}кг", taskId, employeeId, totalWeight);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "❌ Ошибка генерации возврата для курьера {CourierId}", employeeId);
            }
        }
    }
}