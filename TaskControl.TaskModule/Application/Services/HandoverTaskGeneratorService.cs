using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.Application.Services
{
    public class HandoverTaskGeneratorService
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<HandoverTaskGeneratorService> _logger;

        public HandoverTaskGeneratorService(ITaskDataConnection db, ILogger<HandoverTaskGeneratorService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Сценарий 1: Самовывоз (Вызывается по клику из мобильного приложения)
        /// </summary>
        public async Task<int> CreateHandoverToCustomerTaskAsync(int orderId, int initiatingWorkerId, int branchId)
        {
            _logger.LogInformation("Генерация задачи самовывоза для заказа {OrderId} сотрудником {WorkerId}", orderId, initiatingWorkerId);
            return await GenerateTaskCoreAsync(orderId, "ToCustomer", branchId, initiatingWorkerId, null);
        }

        /// <summary>
        /// Сценарий 2: Передача курьеру (Вызывается логистической системой / пакетно)
        /// </summary>
        public async Task<int> CreateHandoverToCourierTaskAsync(int orderId, int courierId, int branchId)
        {
            _logger.LogInformation("Генерация задачи отгрузки курьеру {CourierId} для заказа {OrderId}", courierId, orderId);
            // workerId = null, задача падает в общий пул склада
            return await GenerateTaskCoreAsync(orderId, "ToCourier", branchId, null, courierId);
        }

        private async Task<int> GenerateTaskCoreAsync(int orderId, string handoverType, int branchId, int? specificWorkerId, int? targetCourierId)
        {
            // Проверяем, нет ли уже активной задачи на выдачу этого заказа
            var existingAssignment = await _db.GetTable<OrderHandoverAssignmentModel>()
                .FirstOrDefaultAsync(a => a.OrderId == orderId && (a.Status == 0 || a.Status == 1)); // 0=Assigned, 1=InProgress

            if (existingAssignment != null)
                return existingAssignment.TaskId; // Задача уже существует, просто возвращаем её ID

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                // 1. Проверяем вес заказа
                double totalWeight = await CalculateOrderWeightAsync(orderId);
                bool needsHelper = totalWeight >= 50.0; // Тяжеловес

                // 2. Создаем BaseTask
                var baseTaskId = await _db.InsertWithInt32IdentityAsync(new BaseTaskModel
                {
                    BranchId = branchId,
                    Type = "OrderHandover",
                    Title = handoverType == "ToCustomer" ? $"Выдача клиенту #{orderId}" : $"Отгрузка курьеру #{orderId}",
                    Status = specificWorkerId.HasValue ? "Assigned" : "New", // Если курьеру - висит в пуле New
                    PriorityLevel = needsHelper ? 2 : 1,
                    CreatedAt = DateTime.UtcNow
                });

                // 3. Создаем основное назначение
                var mainAssignmentId = await CreateAssignmentAsync(baseTaskId, orderId, handoverType, specificWorkerId, targetCourierId, "Main");
                await CopyOrderLinesToHandoverAsync(orderId, mainAssignmentId);

                // 4. Логика помощника
                if (needsHelper)
                {
                    _logger.LogInformation("Заказ {OrderId} тяжелый ({Weight} кг). Ищем помощника.", orderId, totalWeight);
                    int? helperId = await FindAvailableHelperAsync(branchId, specificWorkerId);

                    if (helperId.HasValue)
                    {
                        var helperAssignmentId = await CreateAssignmentAsync(baseTaskId, orderId, handoverType, helperId, targetCourierId, "Helper");
                        await CopyOrderLinesToHandoverAsync(orderId, helperAssignmentId);
                        _logger.LogInformation("Помощник {HelperId} назначен на задачу {TaskId}", helperId, baseTaskId);
                    }
                    else
                    {
                        _logger.LogWarning("Свободных помощников на филиале {BranchId} нет.", branchId);
                    }
                }

                await transaction.CommitAsync();
                return baseTaskId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при генерации задачи выдачи для заказа {OrderId}", orderId);
                throw;
            }
        }

        private async Task<int> CreateAssignmentAsync(int taskId, int orderId, string type, int? workerId, int? courierId, string role)
        {
            return await _db.InsertWithInt32IdentityAsync(new OrderHandoverAssignmentModel
            {
                TaskId = taskId,
                OrderId = orderId,
                AssignedToUserId = workerId, // Теперь это nullable!
                HandoverType = type,
                TargetCourierId = courierId,
                Status = workerId.HasValue ? 0 : 0, // 0 = Assigned
                Role = role,
                AssignedAt = DateTime.UtcNow
            });
        }

        private async Task CopyOrderLinesToHandoverAsync(int orderId, int assignmentId)
        {
            var query = from op in _db.GetTable<OrderPositionModel>()
                        join r in _db.GetTable<OrderReservationModel>() on op.UniqueId equals r.OrderPositionId
                        where op.OrderId == orderId && r.ItemPositionId != null
                        select new { op.UniqueId, r.ItemPositionId, op.Quantity };

            var items = await query.ToListAsync();

            foreach (var item in items)
            {
                await _db.InsertAsync(new OrderHandoverLineModel
                {
                    OrderHandoverAssignmentId = assignmentId,
                    OrderPositionId = item.UniqueId,
                    ItemPositionId = item.ItemPositionId.Value,
                    Quantity = item.Quantity,
                    ScannedQuantity = 0
                });
            }
        }

        private async Task<double> CalculateOrderWeightAsync(int orderId)
        {
            var query = from op in _db.GetTable<OrderPositionModel>()
                        join i in _db.GetTable<ItemModel>() on op.ItemId equals i.ItemId
                        where op.OrderId == orderId
                        select op.Quantity * i.Weight;
            return (await query.ToListAsync()).Sum();
        }

        private async Task<int?> FindAvailableHelperAsync(int branchId, int? excludeWorkerId)
        {
            // WMS Логика: Найти сотрудника на смене, не на перерыве, с ролью складского рабочего

            // 1. Кто сейчас зачекинен на складе (последняя запись - IN)
            var checkedInWorkers = await _db.GetTable<CheckIOEmployeeModel>()
                .Where(c => c.BranchId == branchId)
                .GroupBy(c => c.EmployeeId)
                .Select(g => g.OrderByDescending(x => x.CheckTimeStamp).FirstOrDefault())
                .Where(c => c != null && c.CheckType == "in")
                .Select(c => c.EmployeeId)
                .ToListAsync();

            // 2. Ищем среди них подходящего
            var query = from u in _db.GetTable<MobileAppUserModel>()
                        join e in _db.GetTable<EmployeeModel>() on u.EmployeeId equals e.EmployeesId
                        where checkedInWorkers.Contains(e.EmployeesId)
                              && u.IsOnBreak == false // Не на перерыве
                              && u.EmployeeId != excludeWorkerId // Не инициатор
                              && (e.RoleId == 1 || e.RoleId == 3) // 1=Грузчик/Сборщик
                        select e.EmployeesId;

            var availableHelpers = await query.ToListAsync();

            if (!availableHelpers.Any()) return null;

            // 3. Выбираем того, у кого меньше всего активных задач (ActiveAssignedTasks)
            var workloads = await _db.GetTable<BaseTaskModel>()
                .Where(a => availableHelpers.Contains(a.UserId) && a.CompletedAt == null)
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Сортируем по загрузке, берем самого свободного
            var bestHelperId = availableHelpers
                .OrderBy(id => workloads.FirstOrDefault(w => w.UserId == id)?.Count ?? 0)
                .First();

            return bestHelperId;
        }
    }
}