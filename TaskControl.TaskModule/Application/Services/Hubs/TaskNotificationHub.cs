using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.Services.Hubs
{
    public class TaskNotificationHub : Hub
    {
        private readonly ILogger<TaskNotificationHub> _logger;

        public TaskNotificationHub(ILogger<TaskNotificationHub> logger)
        {
            _logger = logger;
        }

        public async Task RegisterWorker(int workerId)
        {
            _logger.LogInformation("Попытка регистрации... WorkerId: {WorkerId}, ConnectionId: {ConnectionId}", workerId, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, workerId.ToString());
            _logger.LogInformation("WorkerId: {WorkerId} успешно добавлен в группу", workerId);
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("Новое SignalR подключение! ConnectionId: {ConnectionId}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }
}