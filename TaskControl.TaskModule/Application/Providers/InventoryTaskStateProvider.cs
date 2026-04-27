using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Repositories;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Providers
{
    public class InventoryTaskStateProvider : ITaskStateProvider
    {
        private readonly IInventoryAssignmentRepository _repository;
        private readonly ILogger<InventoryTaskStateProvider> _logger;

        public InventoryTaskStateProvider(
            IInventoryAssignmentRepository repository,
            ILogger<InventoryTaskStateProvider> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task PauseActiveTasksAsync(int workerId, int excludeTaskId)
        {
            _logger.LogInformation("Поиск активных инвентаризаций для паузы. Пользователь: {WorkerId}", workerId);

            var assignments = await _repository.GetByUserIdAsync(workerId);

            var activeAssignments = assignments.Where(a =>
                a.Status == AssignmentStatus.InProgress && a.TaskId != excludeTaskId).ToList();

            foreach (var assignment in activeAssignments)
            {
                _logger.LogInformation("Пауза инвентаризации (ID: {Id})", assignment.Id);
                assignment.Status = AssignmentStatus.Paused;
                await _repository.UpdateAsync(assignment);
            }
        }

        public async Task<bool> TryActivateTaskAsync(int taskId, int workerId)
        {
            var assignment = await _repository.GetByTaskIdAsync(taskId);

            if (assignment != null && assignment.AssignedToUserId == workerId)
            {
                _logger.LogInformation("Возобновление инвентаризации. TaskID: {TaskId}", taskId);
                assignment.Status = AssignmentStatus.InProgress;
                await _repository.UpdateAsync(assignment);
                return true;
            }

            return false;
        }
    }
}