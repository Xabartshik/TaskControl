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

        public OrderAssemblyTaskStateProvider(IOrderAssemblyAssignmentRepository repository)
        {
            _repository = repository;
        }

        public async Task PauseActiveTasksAsync(int workerId, int excludeTaskId)
        {
            // Получаем назначения через существующий метод
            var assignments = await _repository.GetByUserIdAsync(workerId);

            // Фильтруем те, что InProgress (1) и не являются текущей задачей
            var activeAssignments = assignments.Where(a =>
                a.Status == AssignmentStatus.InProgress && a.TaskId != excludeTaskId);

            foreach (var assignment in activeAssignments)
            {
                // Устанавливаем статус Paused (2)
                assignment.Status = AssignmentStatus.Paused;
                await _repository.UpdateAsync(assignment);
            }
        }

        public async Task<bool> TryActivateTaskAsync(int taskId, int workerId)
        {
            // Ищем назначение по ID задачи
            var assignment = await _repository.GetByTaskIdAsync(taskId);

            if (assignment != null && assignment.AssignedToUserId == workerId)
            {
                assignment.Status = AssignmentStatus.InProgress;
                await _repository.UpdateAsync(assignment);
                return true;
            }
            return false;
        }
    }
}