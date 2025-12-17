using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;

namespace TaskControl.TaskModule.Application.Interface
{
    /// <summary>
    /// Сервис для получения отчётов и аналитики по инвентаризации
    /// </summary>
    public interface IInventoryReportService
    {
        /// <summary>
        /// Получить отчёт о статистике инвентаризации
        /// </summary>
        Task<InventoryStatisticsDto> GetInventoryStatisticsAsync(int assignmentId);

        /// <summary>
        /// Получить отчёт по завершённым инвентаризациям за период
        /// </summary>
        Task<List<CompletedInventoryReportDto>> GetCompletedInventoriesAsync(DateTime from, DateTime to);

        /// <summary>
        /// Получить отчёт эффективности по работникам
        /// </summary>
        Task<WorkerPerformanceReportDto> GetWorkerPerformanceAsync(int userId, DateTime from, DateTime to);
    }
}
