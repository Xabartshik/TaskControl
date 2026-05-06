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
        private readonly TaskWorkloadAggregator _aggregator; // Внедряем Агрегатор!

        public HandoverTaskGeneratorService(
            ITaskDataConnection db,
            ILogger<HandoverTaskGeneratorService> logger,
            TaskWorkloadAggregator aggregator) // Внедряем Агрегатор!
        {
            _db = db;
            _logger = logger;
            _aggregator = aggregator;
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
            var existingAssignment = await _db.GetTable<OrderHandoverAssignmentModel>()
                .FirstOrDefaultAsync(a => a.OrderId == orderId && (a.Status == 0 || a.Status == 1));

            if (existingAssignment != null) return existingAssignment.TaskId;

            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                // 1. Проверяем вес заказа
                double totalWeight = await CalculateOrderWeightAsync(orderId) / 1000;
                bool needsHelper = totalWeight >= 50.0;
                double weightCoef = 0.05;
                double mainComplexity = 1.0 + (totalWeight * weightCoef);
                double helperComplexity = needsHelper ? (0.5 + ((totalWeight / 2) * weightCoef)) : 0;
                // Блокировка помощника для курьеров (если заказ выдает курьер клиенту)
                if (needsHelper && specificWorkerId.HasValue && handoverType == "ToCustomer")
                {
                    var worker = await _db.GetTable<EmployeeModel>().FirstOrDefaultAsync(e => e.EmployeesId == specificWorkerId.Value);
                    if (worker != null && worker.RoleId == 4) // 5 = Courier
                    {
                        needsHelper = false;
                        _logger.LogInformation("Выдачу клиенту инициировал Курьер {WorkerId}. Назначение помощника отменено.", specificWorkerId);
                    }
                }

                // 2. Создаем BaseTask
                var baseTaskId = await _db.InsertWithInt32IdentityAsync(new BaseTaskModel
                {
                    BranchId = branchId,
                    Type = "OrderHandover",
                    Title = handoverType == "ToCustomer" ? $"Выдача клиенту #{orderId}" : $"Отгрузка курьеру #{orderId}",
                    Status = specificWorkerId.HasValue ? "Assigned" : "New",
                    PriorityLevel = needsHelper ? 2 : 1,
                    CreatedAt = DateTime.UtcNow
                });

                // 3. Создаем основное назначение и копируем товары ТОЛЬКО ДЛЯ НЕГО
                var mainAssignmentId = await CreateAssignmentAsync(baseTaskId, orderId, handoverType, specificWorkerId, targetCourierId, "Main", mainComplexity);
                await CopyOrderLinesToHandoverAsync(orderId, mainAssignmentId);

                // 4. Логика помощника
                if (needsHelper)
                {
                    _logger.LogInformation("Заказ {OrderId} тяжелый ({Weight} кг). Запрос помощника через агрегатор.", orderId, totalWeight);

                    int? helperId = await _aggregator.FindAvailableHelperAsync(branchId, specificWorkerId);

                    if (helperId.HasValue)
                    {
                        // Создаем назначение для помощника, но НЕ КОПИРУЕМ ЕМУ СТРОКИ ТОВАРОВ
                        await CreateAssignmentAsync(baseTaskId, orderId, handoverType, helperId, targetCourierId, "Helper", helperComplexity);
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

        public async Task<int> CreateBatchHandoverToCourierTaskAsync(List<int> orderIds, int courierId, int branchId)
        {
            var conn = (DataConnection)_db;
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // 1. Считаем ОБЩИЙ вес всех заказов в маршрутном листе
                double totalWeight = await CalculateBatchWeightAsync(orderIds) / 1000;
                bool needsHelper = totalWeight >= 50.0;
                // --- РАСЧЕТ СЛОЖНОСТИ ПАКЕТА ---
                double weightCoef = 0.05;
                // Базовая сложность пакета = количество заказов + влияние веса
                double mainBatchComplexity = orderIds.Count + (totalWeight * weightCoef);
                double helperBatchComplexity = needsHelper ? (orderIds.Count * 0.5 + ((totalWeight / 2) * weightCoef)) : 0;

                // Делим сложность ПАКЕТА на количество ЗАКАЗОВ, так как назначения создаются поштучно
                double mainComplexityPerOrder = mainBatchComplexity / orderIds.Count;
                double helperComplexityPerOrder = needsHelper ? (helperBatchComplexity / orderIds.Count) : 0;

                var baseTask = new BaseTaskModel
                {
                    BranchId = branchId,
                    Type = "OrderHandover",
                    Title = $"Отгрузка курьеру #{courierId} ({orderIds.Count} заказов)",
                    Status = "New",
                    PriorityLevel = needsHelper ? 2 : 1,
                    CreatedAt = DateTime.UtcNow
                };

                int taskId = await _db.InsertWithInt32IdentityAsync(baseTask);

                // 2. Ищем помощника
                int? helperId = null;
                if (needsHelper)
                {
                    helperId = await _aggregator.FindAvailableHelperAsync(branchId, null);
                    if (helperId.HasValue)
                    {
                        _logger.LogInformation("Для пакетной отгрузки курьеру (вес {Weight} кг) назначен помощник {HelperId}", totalWeight, helperId);
                    }
                    else
                    {
                        _logger.LogWarning("Свободных помощников на филиале {BranchId} нет.", branchId);
                    }
                }

                // 3. Создаем назначения для заказов
                foreach (var orderId in orderIds)
                {
                    // Главное назначение (Передаем mainComplexityPerOrder)
                    int mainAssignmentId = await CreateAssignmentAsync(
                        taskId, orderId, "ToCourier", null, courierId, "Main", mainComplexityPerOrder);

                    await GenerateHandoverLinesAsync(mainAssignmentId, orderId);

                    // Назначение помощника (Передаем helperComplexityPerOrder)
                    if (helperId.HasValue)
                    {
                        await CreateAssignmentAsync(
                            taskId, orderId, "ToCourier", helperId, courierId, "Helper", helperComplexityPerOrder);
                    }
                }

                await transaction.CommitAsync();

                _logger.LogInformation("Успешно создана пакетная отгрузка TaskId={TaskId} на {Count} заказов", taskId, orderIds.Count);
                return taskId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при генерации пакетной отгрузки для курьера {CourierId}", courierId);
                throw;
            }
        }
        private async Task<int> CreateAssignmentAsync(int taskId, int orderId, string type, int? workerId, int? courierId, string role, double complexity)
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
                Complexity = complexity,
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

        private async Task GenerateHandoverLinesAsync(int assignmentId, int orderId)
        {
            _logger.LogInformation("Генерация строк выдачи для назначения {AssignmentId} (Заказ {OrderId})", assignmentId, orderId);

            // 1. Получаем все позиции заказа (что именно купил клиент)
            var orderPositions = await _db.GetTable<OrderPositionModel>()
                .Where(op => op.OrderId == orderId)
                .ToListAsync();

            if (!orderPositions.Any())
            {
                _logger.LogWarning("Заказ {OrderId} не содержит позиций. Строки выдачи не созданы.", orderId);
                return;
            }

            foreach (var position in orderPositions)
            {
                // 2. Ищем физическое расположение товара.
                // При успешной сборке заказа (OrderAssembly), ItemPositionId в резерве 
                // обновляется на ячейку зоны отгрузки (PICKUP).
                var reservation = await _db.GetTable<OrderReservationModel>()
                    .FirstOrDefaultAsync(r => r.OrderPositionId == position.UniqueId);

                if (reservation == null || !reservation.ItemPositionId.HasValue)
                {
                    _logger.LogWarning("Для позиции заказа {UniqueId} не найден резерв с указанием ячейки (ItemPositionId). Товар не собран?", position.UniqueId);

                    // Мы не можем просить кладовщика отгрузить то, что неизвестно где лежит.
                    throw new InvalidOperationException($"Невозможно создать выдачу: для позиции {position.UniqueId} заказа {orderId} не найдена складская ячейка.");
                }

                // 3. Создаем строку выдачи
                // Убедись, что модель называется именно так (или поменяй под свое название)
                var line = new OrderHandoverLineModel
                {
                    OrderHandoverAssignmentId = assignmentId,
                    OrderPositionId = position.UniqueId,
                    ItemPositionId = reservation.ItemPositionId.Value,
                    Quantity = position.Quantity, // Сколько штук нужно отгрузить
                    ScannedQuantity = 0           // Кладовщик еще ничего не отсканировал
                };

                await _db.InsertAsync(line);
            }
        }

        private async Task<double> CalculateBatchWeightAsync(List<int> orderIds)
        {
            var query = from op in _db.GetTable<OrderPositionModel>()
                        join i in _db.GetTable<ItemModel>() on op.ItemId equals i.ItemId
                        where orderIds.Contains(op.OrderId)
                        select op.Quantity * i.Weight;

            return (await query.ToListAsync()).Sum();
        }

        private async Task CopyBatchOrderLinesToHandoverAsync(List<int> orderIds, int assignmentId)
        {
            // Собираем резервы по всем заказам из списка
            var query = from op in _db.GetTable<OrderPositionModel>()
                        join r in _db.GetTable<OrderReservationModel>() on op.UniqueId equals r.OrderPositionId
                        where orderIds.Contains(op.OrderId) && r.ItemPositionId != null
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

    }
}