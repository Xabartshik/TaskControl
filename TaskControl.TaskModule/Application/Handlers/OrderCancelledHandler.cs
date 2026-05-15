using LinqToDB;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.Domain;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.Application.Handlers
{
    public class OrderCancelledHandler : IOrderCancelledEventHandler
    {
        private readonly ITaskDataConnection _db;
        private readonly IOrderCancellationService _cancellationService;
        private readonly ReturnTaskGeneratorService _returnTaskGenerator;
        private readonly INotificationService _notificationService;

        public OrderCancelledHandler(
            ITaskDataConnection db,
            IOrderCancellationService cancellationService,
            ReturnTaskGeneratorService returnTaskGenerator,
            INotificationService notificationService)
        {
            _db = db;
            _cancellationService = cancellationService;
            _returnTaskGenerator = returnTaskGenerator;
            _notificationService = notificationService;
        }

        public async Task HandleOrderCancelledAsync(int orderId, int branchId, OrderStatus previousStatus)
        {
            //  1. ОТМЕНА АКТИВНЫХ ЗАДАЧ И УВЕДОМЛЕНИЕ СОТРУДНИКОВ 
            var activeAssignments = await _db.GetTable<OrderAssemblyAssignmentModel>()
                .Where(a => a.OrderId == orderId && a.Status != 3 && a.Status != 4) // 3 - Completed, 4 - Cancelled
                .ToListAsync();

            foreach (var assignment in activeAssignments)
            {
                // Отменяем назначение
                await _db.GetTable<OrderAssemblyAssignmentModel>()
                    .Where(a => a.Id == assignment.Id)
                    .Set(a => a.Status, 4) // Статус "Отменена"
                    .UpdateAsync();

                // Отменяем базовую задачу
                await _db.GetTable<BaseTaskModel>()
                    .Where(t => t.TaskId == assignment.TaskId)
                    .Set(t => t.Status, "Cancelled")
                    .UpdateAsync();

                // Если задача была у кого-то в работе, стреляем в SignalR
                if (assignment.AssignedToUserId.HasValue)
                {
                    await _notificationService.NotifyTaskCancelledAsync(
                        assignment.AssignedToUserId.Value,
                        assignment.TaskId,
                        "Сборка заказа"
                    );
                }
            }
            // Статусы: 2 = Completed, 3 = Cancelled
            var handoverAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.OrderId == orderId && a.Status != 2 && a.Status != 3)
                .ToListAsync();

            foreach (var assignment in handoverAssignments)
            {
                // Отменяем конкретное назначение (только этот заказ)
                await _db.GetTable<OrderHandoverAssignmentModel>()
                    .Where(a => a.Id == assignment.Id)
                    .Set(a => a.Status, 3)
                    .UpdateAsync();

                // Проверяем пакетную задачу: остались ли в ней другие активные заказы?
                var hasActiveOrdersInTask = await _db.GetTable<OrderHandoverAssignmentModel>()
                    .AnyAsync(a => a.TaskId == assignment.TaskId && a.Status != 2 && a.Status != 3);

                if (!hasActiveOrdersInTask)
                {
                    // Если это был единственный или последний активный заказ в задаче — отменяем её полностью
                    await _db.GetTable<BaseTaskModel>()
                        .Where(t => t.TaskId == assignment.TaskId)
                        .Set(t => t.Status, "Cancelled")
                        .UpdateAsync();
                }

                // Уведомляем кладовщика (Main или Helper), если они взяли задачу в работу
                if (assignment.AssignedToUserId.HasValue)
                {
                    string taskName = assignment.HandoverType == "ToCourier" ? "Отгрузка курьеру" : "Выдача клиенту";
                    await _notificationService.NotifyTaskCancelledAsync(
                        assignment.AssignedToUserId.Value,
                        assignment.TaskId,
                        taskName
                    );
                }
            } 
            //  2. ОБРАБОТКА РЕЗЕРВОВ И ВОЗВРАТОВ 
            var orderPositions = await _db.GetTable<OrderPositionModel>().Where(op => op.OrderId == orderId).ToListAsync();
            var positionIds = orderPositions.Select(op => op.UniqueId).ToList();

            var reservations = await _db.GetTable<OrderReservationModel>()
                .Where(r => positionIds.Contains(r.OrderPositionId) && r.ItemPositionId != null).ToListAsync();

            var cancelledPositions = reservations.GroupBy(r => r.ItemPositionId.Value)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

            if (!cancelledPositions.Any()) return;

            var itemsToReturn = await _cancellationService.ProcessCancellationAsync(orderId, cancelledPositions, true);

            if ((previousStatus != OrderStatus.Created || previousStatus != OrderStatus.AwaitingPayment) && itemsToReturn.Any())
            {
                await _returnTaskGenerator.GenerateReturnTaskFromCancelledItemsAsync(orderId, branchId, itemsToReturn);
            }
        }
    }
}
