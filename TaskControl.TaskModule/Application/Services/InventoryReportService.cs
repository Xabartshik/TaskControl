using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Repositories;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services
{
    public class InventoryReportService : IInventoryReportService
    {
        private readonly IInventoryStatisticsRepository _statisticsRepository;
        private readonly IInventoryAssignmentRepository _assignmentRepository;
        private readonly ILogger<InventoryReportService> _logger;

        public InventoryReportService(
            IInventoryStatisticsRepository statisticsRepository,
            IInventoryAssignmentRepository assignmentRepository,
            ILogger<InventoryReportService> logger)
        {
            _statisticsRepository = statisticsRepository ?? throw new ArgumentNullException(nameof(statisticsRepository));
            _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<InventoryStatisticsDto> GetInventoryStatisticsAsync(int assignmentId)
        {
            _logger.LogInformation("Получение статистики для назначения {AssignmentId}", assignmentId);

            var statistics = await _statisticsRepository.GetByAssignmentIdAsync(assignmentId);
            if (statistics is null)
                throw new InvalidOperationException($"Статистика для {assignmentId} не найдена");

            return MapToStatisticsDto(statistics);
        }

        public async Task<List<CompletedInventoryReportDto>> GetCompletedInventoriesAsync(DateTime from, DateTime to)
        {
            _logger.LogInformation(
                "Получение завершённых инвентаризаций с {From} по {To}",
                from, to);

            // TODO: Реализовать фильтрацию по периоду
            var reports = new List<CompletedInventoryReportDto>();
            return reports;
        }

        public async Task<WorkerPerformanceReportDto> GetWorkerPerformanceAsync(int userId, DateTime from, DateTime to)
        {
            _logger.LogInformation(
                "Получение отчёта производительности работника {UserId} с {From} по {To}",
                userId, from, to);

            var assignments = await _assignmentRepository.GetByUserIdAsync(userId);
            var completedAssignments = assignments
                .Where(a => a.CompletedAt.HasValue && a.CompletedAt >= from && a.CompletedAt <= to)
                .ToList();

            // TODO: Собрать статистику по завершённым назначениям

            return new WorkerPerformanceReportDto
            {
                UserId = userId,
                PeriodFrom = from,
                PeriodTo = to,
                CompletedInventories = completedAssignments.Count
            };
        }

        private InventoryStatisticsDto MapToStatisticsDto(InventoryStatistics statistics)
        {
            return new InventoryStatisticsDto
            {
                Id = statistics.Id,
                InventoryAssignmentId = statistics.InventoryAssignmentId,
                TotalPositions = statistics.TotalPositions,
                CountedPositions = statistics.CountedPositions,
                CompletionPercentage = statistics.CompletionPercentage,
                DiscrepancyCount = statistics.DiscrepancyCount,
                SurplusCount = statistics.SurplusCount,
                ShortageCount = statistics.ShortageCount,
                TotalSurplusQuantity = statistics.TotalSurplusQuantity,
                TotalShortageQuantity = statistics.TotalShortageQuantity,
                StartedAt = statistics.StartedAt,
                CompletedAt = statistics.CompletedAt,
            };
        }
    }
}
