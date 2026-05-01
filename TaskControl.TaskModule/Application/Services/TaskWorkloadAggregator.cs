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

        public async Task<IEnumerable<MobileBaseTaskDto>> GetAllActiveTasksAsync(int workerId)
        {
            var allActiveTasks = new List<MobileBaseTaskDto>();
            foreach (var provider in _providers)
            {
                var moduleTasks = await provider.GetActiveTasksAsync(workerId);
                allActiveTasks.AddRange(moduleTasks);
            }
            return allActiveTasks;
        }

        public async Task<double> GetTotalActiveComplexityAsync(int workerId)
        {
            double totalComplexity = 0;
            foreach (var provider in _providers)
            {
                totalComplexity += await provider.GetActiveWorkloadComplexityAsync(workerId);
            }
            return totalComplexity;
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

        public async Task<IEnumerable<int>> GetAssignedEmployeeIdsAsync(int taskId)
        {
            var allWorkerIds = new List<int>();

            foreach (var provider in _providers)
            {
                var workerIds = await provider.GetAssignedEmployeeIdsAsync(taskId);
                allWorkerIds.AddRange(workerIds);
            }
            return allWorkerIds.Distinct();
        }

    }
}
