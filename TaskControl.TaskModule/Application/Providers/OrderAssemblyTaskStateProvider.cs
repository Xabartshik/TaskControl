using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Providers
{
    public class OrderAssemblyTaskStateProvider : ITaskStateProvider
    {
        private readonly IOrderAssemblyAssignmentRepository _repository;
        private readonly ILogger<OrderAssemblyTaskStateProvider> _logger;

        public OrderAssemblyTaskStateProvider(
            IOrderAssemblyAssignmentRepository repository,
            ILogger<OrderAssemblyTaskStateProvider> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task PauseActiveTasksAsync(int workerId, int excludeTaskId)
        {
            _logger.LogInformation("Поиск активных сборок заказа для постановки на паузу. Пользователь: {WorkerId}", workerId);

            var assignments = await _repository.GetByUserIdAsync(workerId);

            var activeAssignments = assignments.Where(a =>
                a.Status == AssignmentStatus.InProgress && a.TaskId != excludeTaskId).ToList();

            if (!activeAssignments.Any())
            {
                _logger.LogDebug("Активных сборок для пользователя {WorkerId} не найдено.", workerId);
                return;
            }

            foreach (var assignment in activeAssignments)
            {
                _logger.LogInformation("Ставим на паузу сборку (Assignment ID: {Id}) для задачи {TaskId}", assignment.Id, assignment.TaskId);
                assignment.Status = AssignmentStatus.Paused;
                await _repository.UpdateAsync(assignment);
            }
        }

        public async Task<bool> TryActivateTaskAsync(int taskId, int workerId)
        {
            var assignment = await _repository.GetByTaskIdAsync(taskId);

            if (assignment != null && assignment.AssignedToUserId == workerId)
            {
                _logger.LogInformation("Активация сборки заказа. TaskID: {TaskId}, WorkerID: {WorkerId}", taskId, workerId);
                assignment.Status = AssignmentStatus.InProgress;
                await _repository.UpdateAsync(assignment);
                return true;
            }

            return false;
        }
    }
}