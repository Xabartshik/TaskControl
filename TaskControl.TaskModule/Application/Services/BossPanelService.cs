using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskControl.InformationModule.Application.Services;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Repositories;

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
        private readonly ILogger<BossPanelService> _logger;

        public BossPanelService(
            IInventoryProcessService inventoryProcessService,
            IInventoryReportService inventoryReportService,
            IDiscrepancyManagementService discrepancyManagementService,
            ActiveEmployeeService activeEmployeeService,
            IInventoryAssignmentRepository assignmentRepository,
            ILogger<BossPanelService> logger)
        {
            _inventoryProcessService = inventoryProcessService ?? throw new ArgumentNullException(nameof(inventoryProcessService));
            _inventoryReportService = inventoryReportService ?? throw new ArgumentNullException(nameof(inventoryReportService));
            _discrepancyManagementService = discrepancyManagementService ?? throw new ArgumentNullException(nameof(discrepancyManagementService));
            _activeEmployeeService = activeEmployeeService ?? throw new ArgumentNullException(nameof(activeEmployeeService));
            _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
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
    }
}
