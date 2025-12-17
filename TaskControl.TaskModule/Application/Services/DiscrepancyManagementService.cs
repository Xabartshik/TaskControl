using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services
{
    public class DiscrepancyManagementService : IDiscrepancyManagementService
    {
        private readonly IInventoryDiscrepancyRepository _discrepancyRepository;
        private readonly ILogger<DiscrepancyManagementService> _logger;

        public DiscrepancyManagementService(
            IInventoryDiscrepancyRepository discrepancyRepository,
            ILogger<DiscrepancyManagementService> logger)
        {
            _discrepancyRepository = discrepancyRepository ?? throw new ArgumentNullException(nameof(discrepancyRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DiscrepancyReportDto> GetDiscrepanciesAsync(int inventoryAssignmentId)
        {
            _logger.LogInformation("Получение расхождений для назначения {AssignmentId}", inventoryAssignmentId);

            var discrepancies = await _discrepancyRepository.GetByAssignmentIdAsync(inventoryAssignmentId);

            var report = new DiscrepancyReportDto { InventoryAssignmentId = inventoryAssignmentId };
            foreach (var d in discrepancies)
            {
                report.Discrepancies.Add(DiscrepancyDto.ToDto(d));
            }

            return report;
        }

        public async Task<List<DiscrepancyDto>> GetPendingDiscrepanciesAsync()
        {
            _logger.LogInformation("Получение всех нерешённых расхождений");

            var discrepancies = await _discrepancyRepository.GetPendingAsync();
            return discrepancies.Select(DiscrepancyDto.ToDto).ToList();
        }

        public async Task<DiscrepancyDto> ResolveDiscrepancyAsync(ResolveDiscrepancyDto resolveDto)
        {
            if (resolveDto is null)
                throw new ArgumentNullException(nameof(resolveDto));

            _logger.LogInformation(
                "Разрешение расхождения {DiscrepancyId}, новый статус: {Status}",
                resolveDto.DiscrepancyId, resolveDto.ResolutionStatus);

            var discrepancy = await _discrepancyRepository.GetByIdAsync(resolveDto.DiscrepancyId);
            if (discrepancy is null)
                throw new InvalidOperationException($"Расхождение {resolveDto.DiscrepancyId} не найдено");

            // Установить статус в зависимости от выбора
            switch (resolveDto.ResolutionStatus)
            {
                case DiscrepancyResolutionStatus.Resolved:
                    discrepancy.Resolve(resolveDto.Reason);
                    break;
                case DiscrepancyResolutionStatus.UnderInvestigation:
                    discrepancy.MarkForInvestigation(resolveDto.Reason);
                    break;
                case DiscrepancyResolutionStatus.WrittenOff:
                    discrepancy.MarkAsWrittenOff(resolveDto.Reason);
                    break;
                default:
                    throw new InvalidOperationException($"Неизвестный статус: {resolveDto.ResolutionStatus}");
            }

            await _discrepancyRepository.UpdateAsync(discrepancy);
            _logger.LogInformation("Расхождение {DiscrepancyId} разрешено", resolveDto.DiscrepancyId);

            return DiscrepancyDto.ToDto(discrepancy);
        }

        public async Task<DiscrepancyAnalyticsDto> GetDiscrepancyAnalyticsAsync(
            int branchId,
            DateTime from,
            DateTime to)
        {
            _logger.LogInformation(
                "Получение аналитики расхождений для филиала {BranchId} с {From} по {To}",
                branchId, from, to);

            // TODO: Получить расхождения за период и рассчитать метрики
            var analytics = new DiscrepancyAnalyticsDto
            {
            };

            return analytics;
        }


    }

}
