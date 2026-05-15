using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.TaskModule.Application.DTOs.BossPanelDTOs;
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
        /// Автоматический подбор наименее загруженных сотрудников на филиале с возможностью фильтрации.
        /// </summary>
        public async Task<IEnumerable<int>> GetAutoSelectedEmployeesAsync(
            int branchId,
            int requiredCount,
            IEnumerable<int> excludedEmployeeIds = null,
            IEnumerable<int> excludedRoleIds = null)
        {
            // 1. Кто сейчас зачекинен на складе (последняя запись - IN)
            // ИСПРАВЛЕНО: Транслируемый в SQL подзапрос для поиска последней отметки каждого сотрудника
            var checkedInWorkers = await _db.GetTable<EmployeeModel>()
                .Where(e => _db.GetTable<CheckIOEmployeeModel>()
                    .Where(c => c.EmployeeId == e.EmployeesId && c.BranchId == branchId)
                    .OrderByDescending(c => c.CheckTimeStamp)
                    .Select(c => c.CheckType)
                    .FirstOrDefault() == "in")
                .Select(e => e.EmployeesId)
                .ToListAsync();

            if (!checkedInWorkers.Any())
                return new List<int>();

            // 2. Формируем базовый запрос: берем тех, кто на смене и не на перерыве
            var query = from u in _db.GetTable<MobileAppUserModel>()
                        join e in _db.GetTable<EmployeeModel>() on u.EmployeeId equals e.EmployeesId
                        where checkedInWorkers.Contains(e.EmployeesId)
                              && u.IsOnBreak == false
                        select new { e.EmployeesId, e.RoleId };

            // 2.1 Применяем опциональный фильтр по ID сотрудника
            if (excludedEmployeeIds != null && excludedEmployeeIds.Any())
            {
                var exclIds = excludedEmployeeIds.ToList();
                query = query.Where(x => !exclIds.Contains(x.EmployeesId));
            }

            // 2.2 Применяем опциональный фильтр по ролям (например, отсекаем начальников и курьеров)
            if (excludedRoleIds != null && excludedRoleIds.Any())
            {
                var exclRoles = excludedRoleIds.ToList();
                query = query.Where(x => !exclRoles.Contains(x.RoleId));
            }

            // Выполняем запрос к БД
            var candidateIds = await query.Select(x => x.EmployeesId).ToListAsync();

            if (!candidateIds.Any())
                return new List<int>();

            // 3. Считаем реальную загрузку (сложность) каждого кандидата через всех провайдеров
            var workLoads = new List<(int EmployeeId, double Complexity)>();

            foreach (var empId in candidateIds)
            {
                // Используем метод агрегатора для получения общей сложности
                double complexity = await GetTotalActiveComplexityAsync(empId);
                workLoads.Add((empId, complexity));
            }

            // 4. Сортируем: от наименее загруженного к наиболее загруженному, и берем нужное количество
            var selectedIds = workLoads
                .OrderBy(w => w.Complexity)
                .Take(requiredCount)
                .Select(w => w.EmployeeId)
                .ToList();

            return selectedIds;
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
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    // ВАЖНО: Сначала Select(CheckType), только потом FirstOrDefault()
                    LastCheckType = g.OrderByDescending(x => x.CheckTimeStamp)
                                   .Select(x => x.CheckType)
                                   .FirstOrDefault()
                })
                .Where(x => x.LastCheckType == "in")
                .Select(x => x.EmployeeId)
                .ToListAsync();

            if (!checkedInWorkers.Any()) return null;

            // 2. Ищем среди них подходящего
            var availableHelpers = await (from u in _db.GetTable<MobileAppUserModel>()
                                          join e in _db.GetTable<EmployeeModel>() on u.EmployeeId equals e.EmployeesId
                                          where checkedInWorkers.Contains(e.EmployeesId)
                                                && u.IsOnBreak == false // Не на перерыве
                                                && u.EmployeeId != excludeWorkerId // ИСКЛЮЧАЕМ ИНИЦИАТОРА ЗАДАЧИ
                                                && (e.RoleId == 1 || e.RoleId == 2) // 1=Грузчик/Сборщик
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

        public async Task<IEnumerable<EmployeeWorkloadDto>> GetBranchWorkloadAsync(int branchId)
        {
            // 1. Получаем всех сотрудников филиала и их статус чекина
            var employees = await (from e in _db.GetTable<EmployeeModel>()
                                   where _db.GetTable<MobileAppUserModel>().Any(u => u.EmployeeId == e.EmployeesId)
                                   select new
                                   {
                                       e.EmployeesId,
                                       FullName = e.Surname + " " + e.Name,
                                       // Проверка, зачекинен ли он (последний статус "in")
                                       IsAtWork = _db.GetTable<CheckIOEmployeeModel>()
                                           .Where(c => c.EmployeeId == e.EmployeesId && c.BranchId == branchId)
                                           .OrderByDescending(c => c.CheckTimeStamp)
                                           .Select(c => c.CheckType)
                                           .FirstOrDefault() == "in"
                                   }).ToListAsync();

            var result = new List<EmployeeWorkloadDto>();

            foreach (var emp in employees)
            {
                var workload = new EmployeeWorkloadDto
                {
                    EmployeeId = emp.EmployeesId,
                    FullName = emp.FullName,
                    IsAtWork = emp.IsAtWork,
                    TotalComplexity = await GetTotalActiveComplexityAsync(emp.EmployeesId)
                };

                // Собираем краткую информацию о задачах от всех провайдеров
                var activeTasks = await GetAllActiveTasksAsync(emp.EmployeesId);
                workload.ActiveTasksCount = activeTasks.Count();
                workload.ActiveTasks = activeTasks.Select(t => new ActiveTaskBriefDto
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    TaskType = t.TaskType,
                    Status = t.Status.ToString()
                }).ToList();

                result.Add(workload);
            }

            return result.OrderByDescending(r => r.IsAtWork).ThenByDescending(r => r.TotalComplexity);
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

        /// <summary>
        /// Получить все ничейные задачи (Общий пул склада)
        /// </summary>
        public async Task<IEnumerable<MobileBaseTaskDto>> GetGlobalPoolTasksAsync(int branchId)
        {
            var poolTasks = new List<MobileBaseTaskDto>();
            foreach (var provider in _providers)
            {
                // Спрашиваем у каждого модуля: "Есть ли у тебя задачи без исполнителя для этого филиала?"
                var tasks = await provider.GetUnassignedPoolTasksAsync(branchId);
                poolTasks.AddRange(tasks);
            }

            // Сортируем: сначала самые важные, затем самые старые
            return poolTasks.OrderByDescending(t => t.PriorityLevel).ThenBy(t => t.CreatedAt);
        }

    }
}

