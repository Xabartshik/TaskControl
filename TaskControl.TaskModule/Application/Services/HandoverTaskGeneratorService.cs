using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain; // Для ITaskComplexityCalculator

namespace TaskControl.TaskModule.Application.Services
{
    public class HandoverTaskGeneratorService
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<HandoverTaskGeneratorService> _logger;
        private readonly ITaskComplexityCalculator _complexityCalculator; // Новый сервис расчета

        public HandoverTaskGeneratorService(
            ITaskDataConnection db,
            ILogger<HandoverTaskGeneratorService> logger,
            ITaskComplexityCalculator complexityCalculator) 
        {
            _db = db;
            _logger = logger;
            _complexityCalculator = complexityCalculator;
        }

        public async Task<int> CreateHandoverToCustomerTaskAsync(int orderId, int initiatingWorkerId, int branchId)
        {
            _logger.LogInformation("Генерация задачи самовывоза для заказа {OrderId} сотрудником {WorkerId}", orderId, initiatingWorkerId);
            return await GenerateTaskCoreAsync(orderId, "ToCustomer", branchId, initiatingWorkerId, null);
        }

        public async Task<int> CreateHandoverToCourierTaskAsync(int orderId, int courierId, int branchId)
        {
            _logger.LogInformation("Генерация задачи отгрузки курьеру {CourierId} для заказа {OrderId}", courierId, orderId);
            return await GenerateTaskCoreAsync(orderId, "ToCourier", branchId, null, courierId);
        }

        private async Task<int> GenerateTaskCoreAsync(int orderId, string handoverType, int branchId, int? specificWorkerId, int? targetCourierId)
        {
            var existingAssignment = await _db.GetTable<OrderHandoverAssignmentModel>()
                .FirstOrDefaultAsync(a => a.OrderId == orderId && (a.Status == 0 || a.Status == 1));

            if (existingAssignment != null) return existingAssignment.TaskId;

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                // 1. Получаем метрики и рассчитываем сложность через калькулятор
                var metrics = await GetMetricsForOrdersAsync(new List<int> { orderId });
                var result = _complexityCalculator.CalculateForItems(metrics);

                bool needsHelper = result.RequiresHelper;

                // Блокировка помощника для курьеров
                if (needsHelper && specificWorkerId.HasValue && handoverType == "ToCustomer")
                {
                    var worker = await _db.GetTable<EmployeeModel>().FirstOrDefaultAsync(e => e.EmployeesId == specificWorkerId.Value);
                    if (worker != null && worker.RoleId == 4) 
                    {
                        needsHelper = false;
                        _logger.LogInformation("Выдачу инициировал Курьер. Напарник не требуется.");
                    }
                }

                // 2. Создаем BaseTask
                var baseTaskId = await _db.InsertWithInt32IdentityAsync(new BaseTaskModel
                {
                    BranchId = branchId,
                    Type = "OrderHandover",
                    Title = needsHelper ? $"[ТЯЖЕЛЫЙ] Выдача #{orderId}" : $"Выдача #{orderId}",
                    Status = specificWorkerId.HasValue ? "Assigned" : "New",
                    PriorityLevel = needsHelper ? 2 : 1,
                    CreatedAt = DateTime.UtcNow
                });

                // 3. Создаем основное назначение
                // Если помощник был отменен ролью курьера, отдаем Main всю сложность задачи
                double mainComplexity = needsHelper ? result.MainComplexity : result.TotalComplexity;

                var mainAssignmentId = await CreateAssignmentAsync(baseTaskId, orderId, handoverType, specificWorkerId, targetCourierId, "Main", mainComplexity);
                await CopyOrderLinesToHandoverAsync(orderId, mainAssignmentId);

                // 4. Логика помощника: Создаем "пустое" назначение (вакансию)
                if (needsHelper)
                {
                    await CreateAssignmentAsync(baseTaskId, orderId, handoverType, null, targetCourierId, "Helper", result.HelperComplexity);
                    _logger.LogInformation("Для задачи {TaskId} создана вакансия помощника (Complexity: {Complexity})", baseTaskId, result.HelperComplexity);
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

        public async Task<int> CreateBatchHandoverToCourierTaskAsync(List<int> orderIds, int courierId, int branchId)
        {
            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                // 1. Считаем сложность для всего пакета заказов
                var metrics = await GetMetricsForOrdersAsync(orderIds);
                var result = _complexityCalculator.CalculateForItems(metrics);

                var baseTask = new BaseTaskModel
                {
                    BranchId = branchId,
                    Type = "OrderHandover",
                    Title = result.RequiresHelper 
                        ? $"[ТЯЖЕЛЫЙ] Отгрузка #{courierId} ({orderIds.Count} зак.)" 
                        : $"Отгрузка #{courierId} ({orderIds.Count} зак.)",
                    Status = "New",
                    PriorityLevel = result.RequiresHelper ? 2 : 1,
                    CreatedAt = DateTime.UtcNow
                };

                int taskId = await _db.InsertWithInt32IdentityAsync(baseTask);

                // Распределяем сложность пакета на количество заказов для записи в БД
                double mainCompPerOrder = Math.Round(result.MainComplexity / orderIds.Count, 2);
                double helperCompPerOrder = result.RequiresHelper ? Math.Round(result.HelperComplexity / orderIds.Count, 2) : 0;

                // 2. Создаем назначения для каждого заказа
                foreach (var orderId in orderIds)
                {
                    int mainAssignmentId = await CreateAssignmentAsync(taskId, orderId, "ToCourier", null, courierId, "Main", mainCompPerOrder);
                    await GenerateHandoverLinesAsync(mainAssignmentId, orderId);

                    if (result.RequiresHelper)
                    {
                        // Создаем вакансию помощника (без привязки к пользователю)
                        await CreateAssignmentAsync(taskId, orderId, "ToCourier", null, courierId, "Helper", helperCompPerOrder);
                    }
                }

                await transaction.CommitAsync();
                return taskId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при пакетной отгрузке курьера {CourierId}", courierId);
                throw;
            }
        }

        // Вспомогательный метод для сбора метрик товаров
        private async Task<List<TaskItemMetrics>> GetMetricsForOrdersAsync(List<int> orderIds)
        {
            return await (from op in _db.GetTable<OrderPositionModel>()
                          join i in _db.GetTable<ItemModel>() on op.ItemId equals i.ItemId
                          where orderIds.Contains(op.OrderId)
                          select new TaskItemMetrics
                          {
                              WeightGrams = i.Weight,
                              LengthMm = i.Length,
                              WidthMm = i.Width,
                              HeightMm = i.Height,
                              Quantity = op.Quantity
                          }).ToListAsync();
        }

        private async Task<int> CreateAssignmentAsync(int taskId, int orderId, string type, int? workerId, int? courierId, string role, double complexity)
        {
            return await _db.InsertWithInt32IdentityAsync(new OrderHandoverAssignmentModel
            {
                TaskId = taskId,
                OrderId = orderId,
                AssignedToUserId = workerId,
                HandoverType = type,
                TargetCourierId = courierId,
                Status = 0, // 0 = New/Assigned
                Role = role,
                Complexity = complexity,
                AssignedAt = DateTime.UtcNow
            });
        }

        // Остальные методы (CopyOrderLinesToHandoverAsync, GenerateHandoverLinesAsync) остаются без изменений...
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

        private async Task GenerateHandoverLinesAsync(int assignmentId, int orderId)
        {
            var orderPositions = await _db.GetTable<OrderPositionModel>()
                .Where(op => op.OrderId == orderId)
                .ToListAsync();

            foreach (var position in orderPositions)
            {
                var reservation = await _db.GetTable<OrderReservationModel>()
                    .FirstOrDefaultAsync(r => r.OrderPositionId == position.UniqueId);

                if (reservation != null && reservation.ItemPositionId.HasValue)
                {
                    await _db.InsertAsync(new OrderHandoverLineModel
                    {
                        OrderHandoverAssignmentId = assignmentId,
                        OrderPositionId = position.UniqueId,
                        ItemPositionId = reservation.ItemPositionId.Value,
                        Quantity = position.Quantity,
                        ScannedQuantity = 0
                    });
                }
            }
        }
    }
}