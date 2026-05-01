using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services.Hubs;

namespace TaskControl.TaskModule.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<TaskNotificationHub> _hubContext;

        public NotificationService(IHubContext<TaskNotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(int userId, string title, string message, string type = "info")
        {
            var payload = new
            {
                title = title,
                message = message,
                type = type
            };

            await _hubContext.Clients.Group(userId.ToString())
                .SendAsync("ReceiveNotification", payload);
        }

        public async Task NotifyNewTaskAsync(int userId, int taskId, string taskTitle)
        {
            await SendNotificationAsync(userId, "Новая задача!", $"Вам назначена задача: {taskTitle} (#{taskId})", "new_task");
        }

        public async Task NotifyHelperRequiredAsync(int userId, int orderId)
        {
            await SendNotificationAsync(userId, "Нужна помощь!", $"Сборка крупного заказа #{orderId}. Возьмите грузовую тележку.", "helper_required");
        }

        public async Task NotifyHighPriorityTaskAsync(int userId, int taskId, string taskTitle)
        {
            await SendNotificationAsync(userId, "🔥 СРОЧНАЯ ЗАДАЧА 🔥", $"Максимальный приоритет: {taskTitle} (#{taskId})", "high_priority");
        }
    }
}