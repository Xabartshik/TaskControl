using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;

namespace TaskControl.TaskModule.Application.Interface
{
    /// <summary>
    /// Сервис для анализа и управления расхождениями при инвентаризации
    /// </summary>
    public interface IDiscrepancyManagementService
    {
        /// <summary>
        /// Получить все расхождения для назначения
        /// </summary>
        Task<DiscrepancyReportDto> GetDiscrepanciesAsync(int inventoryAssignmentId);

        /// <summary>
        /// Получить нерешённые расхождения
        /// </summary>
        Task<List<DiscrepancyDto>> GetPendingDiscrepanciesAsync();

        /// <summary>
        /// Решить расхождение
        /// </summary>
        Task<DiscrepancyDto> ResolveDiscrepancyAsync(ResolveDiscrepancyDto resolveDto);

        /// <summary>
        /// Получить статистику по расхождениям
        /// </summary>
        Task<DiscrepancyAnalyticsDto> GetDiscrepancyAnalyticsAsync(int branchId, DateTime from, DateTime to);
    }
}
