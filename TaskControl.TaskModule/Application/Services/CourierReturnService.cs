using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.Application.Services
{
    public interface ICourierReturnService
    {
        Task GenerateReturnsIfAnyAsync(int courierId, int branchId);
    }

    public class CourierReturnService : ICourierReturnService
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<CourierReturnService> _logger;

        public CourierReturnService(ITaskDataConnection db, ILogger<CourierReturnService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task GenerateReturnsIfAnyAsync(int courierId, int branchId)
        {
            var db = (DataConnection)_db;

            // 1. Ищем багажник именно этого курьера
            var courierCell = await db.GetTable<PositionModel>()
                .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == courierId.ToString());

            if (courierCell == null) return; // Это не курьер или у него нет ячейки

            // 2. Ищем "сирот" (товары в его машине, на которые нет резерва)
            var orphansQuery = from ip in db.GetTable<ItemPositionModel>()
                               join i in db.GetTable<ItemModel>() on ip.ItemId equals i.ItemId
                               from r in db.GetTable<OrderReservationModel>().Where(res => res.ItemPositionId == ip.Id).DefaultIfEmpty()
                               where ip.PositionId == courierCell.PositionId && r == null // r == null означает, что резерва нет
                               select new { ItemPosition = ip, Item = i };

            var orphans = await orphansQuery.ToListAsync();

            if (!orphans.Any()) return; // Возвращать нечего, багажник чист

            using var transaction = await db.BeginTransactionAsync();
            try
            {
                // 3. Считаем общий вес возвратов
                double totalWeight = orphans.Sum(o => o.Item.Weight * o.ItemPosition.Quantity);
                _logger.LogInformation("Генерация задачи возврата для курьера {CourierId}. Найдено {Count} позиций, вес: {Weight} кг.", courierId, orphans.Count, totalWeight);

                // 4. Создаем базовую задачу
                int baseTaskId = await db.InsertWithInt32IdentityAsync(new BaseTaskModel
                {
                    Title = $"Разгрузка возвратов курьера ID:{courierId}",
                    Type = "ReturnToStock",
                    BranchId = branchId,
                    Status = "New",
                    PriorityLevel = 3, // Высокий приоритет, чтобы машина не простаивала
                    CreatedAt = DateTime.UtcNow
                });

                // 5. Создаем назначение для Основного работника
                int mainAssignmentId = await db.InsertWithInt32IdentityAsync(new ReturnAssignmentModel
                {
                    TaskId = baseTaskId,
                    BranchId = branchId,
                    Status = 0,
                    Role = "Main",
                    Complexity = totalWeight > 20 ? 1.5 : 1.0,
                    AssignedAt = DateTime.UtcNow
                });

                // 6. Если тяжелое - призываем Помощника!
                if (totalWeight >= 50.0)
                {
                    await db.InsertAsync(new ReturnAssignmentModel
                    {
                        TaskId = baseTaskId,
                        BranchId = branchId,
                        Status = 0,
                        Role = "Helper",
                        Complexity = 1.0,
                        AssignedAt = DateTime.UtcNow
                    });
                    _logger.LogInformation("Для возврата курьера {CourierId} добавлен Помощник (перевес).", courierId);
                }

                // 7. Добавляем строки для возврата
                foreach (var orphan in orphans)
                {
                    await db.InsertAsync(new ReturnLineModel
                    {
                        ReturnAssignmentId = mainAssignmentId,
                        ItemPositionId = orphan.ItemPosition.Id,
                        TargetPositionId = null, // Кладовщик сам решит, на какую полку положить
                        Quantity = orphan.ItemPosition.Quantity,
                        ScannedQuantity = 0
                    });
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при генерации задачи возврата для курьера {CourierId}", courierId);
            }
        }
    }
}