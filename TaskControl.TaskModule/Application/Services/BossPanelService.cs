using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskControl.InformationModule.Application.Services;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;
using TaskControl.TaskModule.Application.DTOs.BossPanelDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Repositories;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.InventoryModule.Application.DTOs;

namespace TaskControl.TaskModule.Application.Services
{
    /// <summary>
    /// Сервис для панели начальника
    /// </summary>
    public class BossPanelService : IBossPanelService
    {
        private readonly IInventoryProcessService _inventoryProcessService;
        private readonly IInventoryReportService _inventoryReportService;
        private readonly IDiscrepancyManagementService _discrepancyManagementService;
        private readonly ActiveEmployeeService _activeEmployeeService;
        private readonly IInventoryAssignmentRepository _assignmentRepository;
        private readonly IPositionCellRepository _positionCellRepository;
        private readonly IItemPositionRepository _itemPositionRepository;
        private readonly IActiveTaskRepository _activeTaskRepository;
        private readonly ILogger<BossPanelService> _logger;

        public BossPanelService(
            IInventoryProcessService inventoryProcessService,
            IInventoryReportService inventoryReportService,
            IDiscrepancyManagementService discrepancyManagementService,
            ActiveEmployeeService activeEmployeeService,
            IInventoryAssignmentRepository assignmentRepository,
            IPositionCellRepository positionCellRepository,
            IItemPositionRepository itemPositionRepository,
            IActiveTaskRepository activeTaskRepository,
            ILogger<BossPanelService> logger)
        {
            _inventoryProcessService = inventoryProcessService ?? throw new ArgumentNullException(nameof(inventoryProcessService));
            _inventoryReportService = inventoryReportService ?? throw new ArgumentNullException(nameof(inventoryReportService));
            _discrepancyManagementService = discrepancyManagementService ?? throw new ArgumentNullException(nameof(discrepancyManagementService));
            _activeEmployeeService = activeEmployeeService ?? throw new ArgumentNullException(nameof(activeEmployeeService));
            _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
            _positionCellRepository = positionCellRepository ?? throw new ArgumentNullException(nameof(positionCellRepository));
            _itemPositionRepository = itemPositionRepository ?? throw new ArgumentNullException(nameof(itemPositionRepository));
            _activeTaskRepository = activeTaskRepository ?? throw new ArgumentNullException(nameof(activeTaskRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<CompleteInventoryDto> CreateInventoryTaskAsync(CreateInventoryTaskDto dto, int bossBranchId)
        {
            _logger.LogInformation("Начальник филиала {BossBranchId} создает инвентаризацию для филиала {DtoBranchId}", bossBranchId, dto.BranchId);

            if (dto.BranchId != bossBranchId)
            {
                _logger.LogWarning("Отказ: начальник филиала {BossBranchId} попытался создать задачу для филиала {DtoBranchId}", bossBranchId, dto.BranchId);
                throw new InvalidOperationException($"Вы не можете создать инвентаризацию для филиала {dto.BranchId}. Доступен только филиал {bossBranchId}.");
            }

            var availableWorkers = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(dto.BranchId);
            var workerIds = availableWorkers.Select(w => w.EmployeeId).ToList();

            if (!workerIds.Any())
            {
                throw new InvalidOperationException($"В филиале {dto.BranchId} нет активных сотрудников для назначения инвентаризации.");
            }

            return await _inventoryProcessService.CreateAndDistributeInventoryAsync(dto, workerIds);
        }

        public async Task<IEnumerable<WorkerPerformanceReportDto>> GetBranchWorkersPerformanceAsync(int bossBranchId, DateTime from, DateTime to)
        {
            _logger.LogInformation("Получение отчетов производительности сотрудников для филиала {BossBranchId} с {From} по {To}", bossBranchId, from, to);

            var employees = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
            var reports = new List<WorkerPerformanceReportDto>();

            foreach (var emp in employees)
            {
                try
                {
                    var report = await _inventoryReportService.GetWorkerPerformanceAsync(emp.EmployeeId, from, to);
                    reports.Add(report);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось получить отчет для сотрудника {EmployeeId}", emp.EmployeeId);
                }
            }

            return reports;
        }

        public async Task<DiscrepancyReportDto> GetBranchInventoryDiscrepanciesAsync(int bossBranchId, int assignmentId)
        {
            _logger.LogInformation("Начальник филиала {BossBranchId} запрашивает расхождения по назначению {AssignmentId}", bossBranchId, assignmentId);

            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            if (assignment == null)
            {
                throw new InvalidOperationException($"Назначение {assignmentId} не найдено.");
            }

            if (assignment.BranchId != bossBranchId)
            {
                _logger.LogWarning("Отказ: назначение {AssignmentId} относится к филиалу {BranchId}, а не к филиалу начальника {BossBranchId}", assignmentId, assignment.BranchId, bossBranchId);
                throw new InvalidOperationException($"Назначение {assignmentId} не относится к вашему филиалу.");
            }

            return await _discrepancyManagementService.GetDiscrepanciesAsync(assignmentId);
        }
        public async Task<IEnumerable<string>> GetAvailableZonesAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение доступных зон для филиала {BossBranchId}", bossBranchId);

            var cells = await _positionCellRepository.GetByBranchAsync(bossBranchId);
            
            // Возвращаем уникальные префиксы, например "1-ZA-RACK"
            var zones = cells
                .Select(c => $"{c.Code.BranchId}-{c.Code.ZoneCode}-{c.Code.FirstLevelStorageType}")
                .Distinct()
                .OrderBy(z => z)
                .ToList();

            return zones;
        }

        public async Task<CompleteInventoryDto> CreateInventoryByZoneAsync(CreateInventoryByZoneDto dto, int bossBranchId)
        {
            _logger.LogInformation("Создание инвентаризации по зонам для филиала {BossBranchId}", bossBranchId);

            var cells = await _positionCellRepository.GetByBranchAsync(bossBranchId);

            // Ищем все ячейки, которые начинаются с одного из префиксов
            var targetPositionIds = cells
                .Where(c => dto.ZonePrefixes.Any(prefix => c.Code.ToString().StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                .Select(c => c.PositionId)
                .ToList();

            if (!targetPositionIds.Any())
            {
                throw new InvalidOperationException("Не найдено ни одной позиции по указанным зонам.");
            }

            // Получаем ItemPositions для этих позиций
            var allItemPositions = await _itemPositionRepository.GetAllAsync();
            var targetItemPositionIds = allItemPositions
                .Where(ip => targetPositionIds.Contains(ip.PositionId))
                .Select(ip => ip.Id)
                .Distinct()
                .ToList();

            if (!targetItemPositionIds.Any())
            {
                throw new InvalidOperationException("В выбранных зонах нет ни одного привязанного товара.");
            }

            var workerIds = dto.WorkerIds;
            if (workerIds == null || !workerIds.Any())
            {
                // Если работники не переданы, выбираем активных
                var availableWorkers = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
                workerIds = availableWorkers.Select(w => w.EmployeeId).Take(dto.WorkerCount).ToList();

                if (!workerIds.Any())
                {
                    throw new InvalidOperationException($"В филиале {bossBranchId} нет активных сотрудников.");
                }
            }

            var createDto = new CreateInventoryTaskDto
            {
                BranchId = bossBranchId,
                Priority = dto.Priority,
                ItemPositionIds = targetItemPositionIds,
                WorkerCount = workerIds.Count,
                Description = dto.Description ?? $"Инвентаризация зон: {string.Join(", ", dto.ZonePrefixes)}",
                DivisionStrategy = DivisionStrategy.ByQuantity,
                DeadlineDate = dto.DeadlineDate
            };

            return await _inventoryProcessService.CreateAndDistributeInventoryAsync(createDto, workerIds);
        }

        public async Task<IEnumerable<WorkerStatusDto>> GetActiveWorkersStatusAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение статуса работников для филиала {BossBranchId}", bossBranchId);

            var employees = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
            var result = new List<WorkerStatusDto>();

            foreach (var emp in employees)
            {
                var assignments = await _assignmentRepository.GetByUserIdAsync(emp.EmployeeId);
                var activeCount = assignments.Count(a => a.Status != Domain.InventoryAssignmentStatus.Completed && a.Status != Domain.InventoryAssignmentStatus.Cancelled);

                result.Add(new WorkerStatusDto
                {
                    EmployeeId = emp.EmployeeId,
                    FullName = $"{emp.Surname} {emp.Name}",
                    Role = emp.Role,
                    IsWorking = true,
                    ActiveTaskCount = activeCount
                });
            }

            return result;
        }

        public async Task<IEnumerable<TaskReportGroupDto>> GetGroupedTaskReportsAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение сгруппированных отчетов по задачам для филиала {BossBranchId}", bossBranchId);

            var tasks = await _activeTaskRepository.GetByBranchAsync(bossBranchId);
            var result = new List<TaskReportGroupDto>();

            var assignments = await _assignmentRepository.GetByBranchIdAsync(bossBranchId);

            foreach (var task in tasks.OrderByDescending(t => t.CreatedAt))
            {
                var taskAssignments = assignments.Where(a => a.TaskId == task.TaskId).ToList();
                int total = taskAssignments.Count;
                int completed = taskAssignments.Count(a => a.Status == Domain.InventoryAssignmentStatus.Completed);
                
                string progress = total == 0 ? "Нет назначений" : $"{completed}/{total} завершено";

                result.Add(new TaskReportGroupDto
                {
                    TaskId = task.TaskId,
                    Title = task.Title,
                    Status = task.Status.ToString(),
                    CreatedAt = task.CreatedAt,
                    CompletedAt = task.CompletedAt,
                    AssignmentsProgress = progress
                });
            }

            return result;
        }

        public async Task<IEnumerable<BossPanelTaskCardDto>> GetActiveTasksAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение активных задач для филиала {BossBranchId}", bossBranchId);

            var tasks = await _activeTaskRepository.GetByBranchAsync(bossBranchId);
            var assignments = await _assignmentRepository.GetByBranchIdAsync(bossBranchId);

            var activeTasks = tasks.Where(t => t.Status != Domain.TaskStatus.Completed && t.Status != Domain.TaskStatus.Cancelled);
            var result = new List<BossPanelTaskCardDto>();

            foreach(var t in activeTasks)
            {
                var dict = new Dictionary<int, TaskAssigneeProgressDto>();
                var taskAssignments = assignments.Where(a => a.TaskId == t.TaskId).ToList();
                
                // Здесь в идеале мы должны брать из assignmentRepository AssignedVolume, 
                // но пока для инвентаризации это просто 1 assignment = 1 сотрудник = x ячеек.
                // Для простоты, сделаем фейковые объемы на основе статуса.
                foreach(var a in taskAssignments)
                {
                    if(!dict.ContainsKey(a.AssignedToUserId))
                    {
                        var emp = await _activeEmployeeService.GetEmployeeByIdAsync(a.AssignedToUserId);
                        dict[a.AssignedToUserId] = new TaskAssigneeProgressDto
                        {
                            EmployeeId = a.AssignedToUserId,
                            FullName = emp != null ? $"{emp.Surname} {emp.Name}" : $"Работник {a.AssignedToUserId}",
                            AssignedVolume = 1,
                            CompletedVolume = a.Status == Domain.InventoryAssignmentStatus.Completed ? 1 : 0,
                            Status = a.Status == Domain.InventoryAssignmentStatus.InProgress ? "В процессе" 
                                     : a.Status == Domain.InventoryAssignmentStatus.Completed ? "Завершено" : "Ожидается"
                        };
                    }
                    else
                    {
                        dict[a.AssignedToUserId].AssignedVolume += 1;
                        if(a.Status == Domain.InventoryAssignmentStatus.Completed)
                            dict[a.AssignedToUserId].CompletedVolume += 1;
                    }
                }

                var assignees = dict.Values.ToList();
                int totalAssigned = assignees.Sum(x => x.AssignedVolume);
                int totalCompleted = assignees.Sum(x => x.CompletedVolume);
                int progress = totalAssigned > 0 ? (totalCompleted * 100 / totalAssigned) : 0;

                result.Add(new BossPanelTaskCardDto
                {
                    Id = t.TaskId,
                    Title = t.Title,
                    TaskType = t.Type,
                    CreatedAt = t.CreatedAt,
                    ExpectedCompletionDate = null,
                    OverallProgressPercentage = progress,
                    Assignees = assignees
                });
            }

            return result;
        }

        public async Task<IEnumerable<EmployeeWorkloadDto>> GetEmployeeWorkloadAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение загруженности сотрудников филиала {BossBranchId}", bossBranchId);
            var employees = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
            var result = new List<EmployeeWorkloadDto>();

            foreach(var emp in employees)
            {
                var assignments = await _assignmentRepository.GetByUserIdAsync(emp.EmployeeId);
                var activeTasks = assignments.Where(a => a.Status != Domain.InventoryAssignmentStatus.Completed && a.Status != Domain.InventoryAssignmentStatus.Cancelled).ToList();

                var activeTaskDtos = new List<ActiveTaskBriefDto>();
                foreach(var a in activeTasks)
                {
                    var task = await _activeTaskRepository.GetByIdAsync(a.TaskId);
                    if(task != null)
                    {
                        activeTaskDtos.Add(new ActiveTaskBriefDto
                        {
                            TaskId = task.TaskId,
                            Title = task.Title,
                            TaskType = task.Type,
                            Status = a.Status.ToString()
                        });
                    }
                }

                result.Add(new EmployeeWorkloadDto
                {
                    EmployeeId = emp.EmployeeId,
                    FullName = $"{emp.Surname} {emp.Name}",
                    IsAtWork = true,
                    ActiveTasksCount = activeTaskDtos.Count,
                    ActiveTasks = activeTaskDtos
                });
            }
            return result;
        }

        public async Task<IEnumerable<AvailableEmployeeDto>> GetAvailableEmployeesAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение доступных сотрудников филиала {BossBranchId}", bossBranchId);
            var employees = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
            var result = new List<AvailableEmployeeDto>();

            foreach (var emp in employees)
            {
                var assignments = await _assignmentRepository.GetByUserIdAsync(emp.EmployeeId);
                var activeCount = assignments.Count(a => a.Status != Domain.InventoryAssignmentStatus.Completed && a.Status != Domain.InventoryAssignmentStatus.Cancelled);

                result.Add(new AvailableEmployeeDto
                {
                    EmployeeId = emp.EmployeeId,
                    FullName = $"{emp.Surname} {emp.Name}",
                    IsAtWork = true,
                    ActiveTasksCount = activeCount,
                    IsRecommended = false
                });
            }

            // Помечаем до 3 сотрудников с минимальным кол-вом задач как "Рекомендовано"
            var recommended = result.OrderBy(x => x.ActiveTasksCount).Take(3).ToList();
            foreach (var r in recommended) r.IsRecommended = true;

            return result;
        }

        public async Task<IEnumerable<PositionCellDto>> GetPositionsAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение позиций для дерева филиала {BossBranchId}", bossBranchId);
            var cells = await _positionCellRepository.GetByBranchAsync(bossBranchId);
            return cells.Select(c => PositionCellDto.ToDto(c)).ToList();
        }
    }
}
