using Hangfire;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.OrderModule.DataAccess.Interface;
using TaskControl.OrderModule.Domain.TaskControl.OrderModule.Domain.Enums;
using TaskControl.TaskModule.Application.Services;

namespace TaskControl.TaskModule.Application.Handlers
{
    public class OrderCreatedHandler : IOrderCreatedEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public OrderCreatedHandler(IOrderRepository orderRepository, IBackgroundJobClient backgroundJobClient)
        {
            _orderRepository = orderRepository;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task HandleOrderCreatedAsync(int orderId, int branchId)
        {
            // 1. Получаем данные заказа, чтобы проверить его тип
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null) return;

            // 2. Логика фильтрации:
            // Если заказ Экспресс или в Постамат — нам нужно создать задачу СЕЙЧАС.
            if (order.DeliveryType == DeliveryType.Express || order.DeliveryType == DeliveryType.Postamat)
            {
                // Используем Hangfire Fire-and-Forget. 
                // Это добавит задачу в очередь, и она начнет выполняться немедленно.
                _backgroundJobClient.Enqueue<OrderAssemblyPlannerJob>(x => x.PlanSingleOrderAsync(orderId));
            }

            // Обычные заказы (Pickup/Delivery) мы здесь игнорируем.
            // Их подберет OrderAssemblyPlannerJob.ExecuteAsync(), когда запустится по расписанию.
        }
    }
}