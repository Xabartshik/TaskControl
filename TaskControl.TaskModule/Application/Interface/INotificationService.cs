using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.Interface
{
    public interface INotificationService
    {
        Task SendNotificationAsync(int userId, string title, string message, string type = "info");
        Task NotifyNewTaskAsync(int userId, int taskId, string taskTitle);
        Task NotifyHelperRequiredAsync(int userId, int orderId);
        Task NotifyHighPriorityTaskAsync(int userId, int taskId, string taskTitle);
    }
}