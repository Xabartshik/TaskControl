using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;

namespace TaskControl.TaskModule.Application.Services
{
    public class TaskWorkloadAggregator
    {
        private readonly IEnumerable<ITaskWorkloadProvider> _providers;

        public TaskWorkloadAggregator(IEnumerable<ITaskWorkloadProvider> providers)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        }

        public async Task<int> GetTotalActiveWorkloadAsync(int workerId)
        {
            int total = 0;
            foreach (var provider in _providers)
            {
                total += await provider.GetActiveWorkloadCountAsync(workerId);
            }
            return total;
        }

        public async Task<bool> HasAnyNewAssignmentsAsync(int workerId)
        {
            foreach (var provider in _providers)
            {
                if (await provider.HasNewAssignmentsAsync(workerId))
                    return true;
            }
            return false;
        }

        public async Task<IEnumerable<MobileBaseTaskDto>> GetAllPendingTasksAsync(int workerId)
        {
            var allTasks = new List<MobileBaseTaskDto>();
            foreach (var provider in _providers)
            {
                var moduleTasks = await provider.GetAvailableTasksAsync(workerId);
                allTasks.AddRange(moduleTasks);
            }
            
            return allTasks.OrderByDescending(t => t.PriorityLevel).ThenBy(t => t.CreatedAt);
        }

        public async Task<bool> StartTaskAsync(int taskId, string taskType, int workerId)
        {
            var provider = _providers.FirstOrDefault(p => p.TaskType == taskType);
            
            if (provider == null)
                throw new ArgumentException($"Провайдер для типа задачи '{taskType}' не зарегистрирован в системе.");

            return await provider.TryStartTaskAsync(taskId, workerId);
        }
    }
}
