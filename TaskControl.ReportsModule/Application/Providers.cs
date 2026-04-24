using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Application.DTOs;
using TaskControl.ReportsModule.Application.Interface;
using TaskControl.ReportsModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Interface; // Подключение репозитория сотрудников для получения ФИО

namespace TaskControl.ReportsModule.DataAccess.Providers
{
    /// <summary>
    /// Провайдер аналитических запросов. 
    /// Отвечает за извлечение данных телеметрии и формирование сводных и детальных отчетов.
    /// </summary>
    public class AnalyticsQueryProvider : IAnalyticsQueryProvider
    {
        private readonly IReportDataConnection _db;
        private readonly IEmployeeRepository _employeeRepository;

        // Внедряем подключение к БД отчетов и репозиторий сотрудников для получения их имен
        public AnalyticsQueryProvider(IReportDataConnection db, IEmployeeRepository employeeRepository)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        }

        public async Task<IEnumerable<TaskGroupReportDto>> GetGroupedBranchReportAsync(int branchId, DateTime? start, DateTime? end)
        {
            // Берем детальный отчет из уже написанного нами метода
            var flatData = await GetDetailedBranchReportAsync(branchId, start, end);

            // Группируем по имени категории и дате (без учета времени)
            var grouped = flatData
                .GroupBy(x => new { x.TaskCategoryDisplayName, Date = x.CompletedAt.Date })
                .Select(g => new TaskGroupReportDto
                {
                    GroupName = $"{g.Key.TaskCategoryDisplayName} (Смена: {g.Key.Date:dd.MM.yyyy})",
                    TotalWorkers = g.Select(w => w.WorkerId).Distinct().Count(),
                    TotalItems = g.Sum(x => x.ItemsProcessed),
                    TotalDurationSeconds = g.Sum(x => x.DurationSeconds),
                    TotalDiscrepancies = g.Sum(x => x.Discrepancies),
                    Workers = g.OrderBy(w => w.CompletedAt).ToList()
                })
                .OrderByDescending(g => g.GroupName)
                .ToList();

            return grouped;
        }

        /// <summary>
        /// Получает сводную статистику эффективности работников (группировка по категориям задач).
        /// </summary>
        public async Task<IEnumerable<WorkerEfficiencyResultDto>> GetWorkerEfficiencyAsync(AnalyticsQueryDto query)
        {
            // Базовый запрос с фильтрацией по обязательному периоду
            var queryable = _db.WorkerTaskEfficiency
                .Where(x => x.CompletedAt >= query.StartDate && x.CompletedAt <= query.EndDate);

            // Динамическое добавление опциональных фильтров
            if (query.WorkerId.HasValue)
                queryable = queryable.Where(x => x.WorkerId == query.WorkerId.Value);

            if (query.BranchId.HasValue)
                queryable = queryable.Where(x => x.BranchId == query.BranchId.Value);

            if (!string.IsNullOrEmpty(query.TaskCategory))
                queryable = queryable.Where(x => x.TaskCategory == query.TaskCategory);

            // Группировка на стороне БД и вычисление агрегатов (сумма, среднее, количество)
            var result = await queryable
                .GroupBy(x => x.TaskCategory)
                .Select(g => new WorkerEfficiencyResultDto
                {
                    TaskCategory = g.Key,
                    TotalTasks = g.Count(),
                    ItemsProcessed = g.Sum(x => x.ItemsProcessed),
                    AverageWaitTimeSeconds = g.Count() > 0 ? g.Sum(x => x.WaitTimeSeconds) / g.Count() : 0,
                    AverageQueueSize = g.Count() > 0 ? g.Sum(x => x.QueueSize) / g.Count() : 0,
                    AverageDurationSeconds = g.Count() > 0 ? g.Sum(x => x.TotalDurationSeconds) / g.Count() : 0,
                    DiscrepanciesFound = g.Sum(x => x.DiscrepanciesFound)
                })
                .ToListAsync();

            return result;
        }

        /// <summary>
        /// Получает общую сводку работы по филиалам.
        /// </summary>
        public async Task<IEnumerable<BranchSummaryResultDto>> GetBranchSummaryAsync(AnalyticsQueryDto query)
        {
            // Базовый запрос с фильтрацией по датам
            var queryable = _db.WorkerTaskEfficiency
                .Where(x => x.CompletedAt >= query.StartDate && x.CompletedAt <= query.EndDate);

            if (query.BranchId.HasValue)
                queryable = queryable.Where(x => x.BranchId == query.BranchId.Value);

            // Агрегация данных по каждому филиалу
            var result = await queryable
                .GroupBy(x => x.BranchId)
                .Select(g => new BranchSummaryResultDto
                {
                    BranchId = g.Key,
                    TotalWorkersActive = g.Select(x => x.WorkerId).Distinct().Count(),
                    TotalTasksCompleted = g.Count(),
                    TotalItemsMoved = g.Sum(x => x.ItemsProcessed),
                    TotalDiscrepancies = g.Sum(x => x.DiscrepanciesFound)
                })
                .ToListAsync();

            return result;
        }

        /// <summary>
        /// Формирует детализированный отчет по всем выполненным задачам в рамках одного филиала.
        /// Даты опциональны (null = за все время).
        /// </summary>
        public async Task<IEnumerable<DetailedTaskReportDto>> GetDetailedBranchReportAsync(int branchId, DateTime? start, DateTime? end)
        {
            var query = _db.WorkerTaskEfficiency.Where(x => x.BranchId == branchId);

            // Динамическое применение фильтров по дате
            if (start.HasValue)
                query = query.Where(x => x.CompletedAt >= start.Value);

            if (end.HasValue)
                query = query.Where(x => x.CompletedAt <= end.Value);

            var records = await query.ToListAsync();

            // Обогащаем сырые данные читаемыми именами сотрудников и локализацией
            return await EnrichWithEmployeeDataAndMapAsync(records);
        }

        /// <summary>
        /// Формирует детализированный отчет по конкретному сотруднику.
        /// </summary>
        public async Task<IEnumerable<DetailedTaskReportDto>> GetDetailedWorkerReportAsync(int workerId, DateTime? start, DateTime? end)
        {
            var query = _db.WorkerTaskEfficiency.Where(x => x.WorkerId == workerId);

            if (start.HasValue)
                query = query.Where(x => x.CompletedAt >= start.Value);

            if (end.HasValue)
                query = query.Where(x => x.CompletedAt <= end.Value);

            var records = await query.ToListAsync();

            return await EnrichWithEmployeeDataAndMapAsync(records);
        }

        /// <summary>
        /// Возвращает список только тех задач, которые относятся к инвентаризации.
        /// </summary>
        public async Task<IEnumerable<DetailedTaskReportDto>> GetCompletedInventoriesAsync(DateTime? start, DateTime? end)
        {
            // Фильтруем строго по категории инвентаризации
            var query = _db.WorkerTaskEfficiency.Where(x => x.TaskCategory == "Inventory");

            if (start.HasValue)
                query = query.Where(x => x.CompletedAt >= start.Value);

            if (end.HasValue)
                query = query.Where(x => x.CompletedAt <= end.Value);

            var records = await query.ToListAsync();

            return await EnrichWithEmployeeDataAndMapAsync(records);
        }


        /// <summary>
        /// Вспомогательный метод. Принимает список сырых записей телеметрии, запрашивает имена работников из БД и формирует красивый DTO для вывода в отчет/PDF.
        /// </summary>
        private async Task<IEnumerable<DetailedTaskReportDto>> EnrichWithEmployeeDataAndMapAsync(
            List<TaskControl.ReportsModule.DataAccess.Model.WorkerTaskEfficiencyModel> records)
        {
            if (!records.Any()) return new List<DetailedTaskReportDto>();

            // 1. Собираем уникальные ID всех работников, которые есть в текущей выборке
            var workerIds = records.Select(r => r.WorkerId).Distinct().ToList();

            // 2. Получаем данные обо всех сотрудниках из модуля InformationModule
            var employees = await _employeeRepository.GetAllAsync();
            var employeesDict = employees
                .Where(e => workerIds.Contains(e.EmployeesId))
                .ToDictionary(e => e.EmployeesId);

            // 3. Маппинг и сборка конечного результата
            var mappedResult = records.Select(x =>
            {
                employeesDict.TryGetValue(x.WorkerId, out var employee);

                // Склеиваем ФИО. Если данных в базе нет, выводим ID
                string fullName = employee != null
                    ? $"{employee.Surname} {employee.Name} {employee.MiddleName}".Trim()
                    : $"Сотрудник ID: {x.WorkerId} (Данные отсутствуют)";

                return new DetailedTaskReportDto
                {
                    TaskId = 0,
                    TaskCategory = x.TaskCategory,
                    TaskCategoryDisplayName = GetHumanReadableCategory(x.TaskCategory),
                    WorkerId = x.WorkerId,
                    WorkerFullName = fullName,
                    CompletedAt = x.CompletedAt,
                    DurationSeconds = x.TotalDurationSeconds,
                    WaitTimeSeconds = x.WaitTimeSeconds,
                    ItemsProcessed = x.ItemsProcessed,
                    Discrepancies = x.DiscrepanciesFound,
                    QueueSize = x.QueueSize
                };
            }).OrderByDescending(x => x.CompletedAt).ToList(); // Сортируем от самых новых к старым

            return mappedResult;
        }

        /// <summary>
        /// Перевод системных кодов категорий задач на русский язык для отображения в отчетах.
        /// </summary>
        private string GetHumanReadableCategory(string categoryCode)
        {
            return categoryCode switch
            {
                "Inventory" => "Инвентаризация",
                "OrderAssembly" => "Сборка заказа",
                "BoxPacking" => "Упаковка товара",
                "Loading" => "Погрузка/Разгрузка",
                "Relocation" => "Внутреннее перемещение",
                _ => categoryCode // Если категория неизвестна, возвращаем как есть
            };
        }
    }
}