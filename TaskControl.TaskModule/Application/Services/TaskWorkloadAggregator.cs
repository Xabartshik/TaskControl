using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.Application.Services
{
    public class TaskWorkloadAggregator
    {
        private readonly IEnumerable<ITaskWorkloadProvider> _providers;
        private readonly ITaskDataConnection _db; // ДОБАВЛЕНО: контекст БД

        public TaskWorkloadAggregator(
            IEnumerable<ITaskWorkloadProvider> providers,
            ITaskDataConnection db) // Инжектим БД
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
            _db = db ?? throw new ArgumentNullException(nameof(db));
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

        public async Task<MobileBaseTaskDto?> GetTaskDetailsAsync(int taskId, int workerId)
        {
            foreach (var provider in _providers)
            {
                // Делегируем запрос деталей конкретному провайдеру
                var taskDetails = await provider.GetTaskDetailsAsync(taskId, workerId);
                if (taskDetails != null)
                {
                    return taskDetails;
                }
            }

            return null;
        }

        /// <summary>
        /// Поиск наименее загруженного помощника на филиале
        /// </summary>
        public async Task<int?> FindAvailableHelperAsync(int branchId, int? excludeWorkerId)
        {
            // 1. Кто сейчас зачекинен на складе (последняя запись - IN)
            var checkedInWorkers = await _db.GetTable<CheckIOEmployeeModel>()
                .Where(c => c.BranchId == branchId)
                .GroupBy(c => c.EmployeeId)
                .Select(g => g.OrderByDescending(x => x.CheckTimeStamp).FirstOrDefault())
                .Where(c => c != null && c.CheckType == "in")
                .Select(c => c.EmployeeId)
                .ToListAsync();

            if (!checkedInWorkers.Any()) return null;

            // 2. Ищем среди них подходящего
            var availableHelpers = await (from u in _db.GetTable<MobileAppUserModel>()
                                          join e in _db.GetTable<EmployeeModel>() on u.EmployeeId equals e.EmployeesId
                                          where checkedInWorkers.Contains(e.EmployeesId)
                                                && u.IsOnBreak == false // Не на перерыве
                                                && u.EmployeeId != excludeWorkerId // ИСКЛЮЧАЕМ ИНИЦИАТОРА ЗАДАЧИ
                                                && (e.RoleId == 1 || e.RoleId == 3) // 1=Грузчик/Сборщик
                                          select e.EmployeesId).ToListAsync();

            if (!availableHelpers.Any()) return null;

            // 3. Считаем реальную загрузку каждого кандидата через ВСЕХ провайдеров
            int? bestHelperId = null;
            int minWorkload = int.MaxValue;

            foreach (var helperId in availableHelpers)
            {
                // Используем твой же метод агрегатора, который опрашивает все модули!
                int workload = await GetTotalActiveWorkloadAsync(helperId);

                if (workload < minWorkload)
                {
                    minWorkload = workload;
                    bestHelperId = helperId;
                }
            }

            return bestHelperId;
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

