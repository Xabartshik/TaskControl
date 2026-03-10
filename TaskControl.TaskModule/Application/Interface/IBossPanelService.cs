using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;

namespace TaskControl.TaskModule.Application.Interface
{
    /// <summary>
    /// Интерфейс сервиса панели начальника для управления задачами своего филиала
    /// </summary>
    public interface IBossPanelService
    {
        /// <summary>
        /// Создать задачу инвентаризации в рамках филиала начальника
        /// </summary>
        Task<CompleteInventoryDto> CreateInventoryTaskAsync(CreateInventoryTaskDto dto, int bossBranchId);

        /// <summary>
        /// Получить отчеты производительности сотрудников филиала начальника
        /// </summary>
        Task<IEnumerable<WorkerPerformanceReportDto>> GetBranchWorkersPerformanceAsync(int bossBranchId, DateTime from, DateTime to);

        /// <summary>
        /// Получить расхождения инвентаризации, если она относится к филиалу начальника
        /// </summary>
        Task<DiscrepancyReportDto> GetBranchInventoryDiscrepanciesAsync(int bossBranchId, int assignmentId);
    }
}
