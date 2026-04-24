using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;
using TaskControl.TaskModule.Application.DTOs.BossPanelDTOs;
using TaskControl.InventoryModule.Application.DTOs;

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

        /// <summary>
        /// Получить список доступных зон (префиксов) для инвентаризации
        /// </summary>
        Task<IEnumerable<string>> GetAvailableZonesAsync(int bossBranchId);

        /// <summary>
        /// Создать инвентаризацию на основе выбранных зон (префиксов)
        /// </summary>
        Task<CompleteInventoryDto> CreateInventoryByZoneAsync(CreateInventoryByZoneDto dto, int bossBranchId);

        /// <summary>
        /// Получить статус текущих работников (на смене и их нагрузка)
        /// </summary>
        Task<IEnumerable<WorkerStatusDto>> GetActiveWorkersStatusAsync(int bossBranchId);

        /// <summary>
        /// Получить сгруппированные отчеты по задачам филиала
        /// </summary>
        Task<IEnumerable<TaskReportGroupDto>> GetGroupedTaskReportsAsync(int bossBranchId);

        Task<IEnumerable<BossPanelTaskCardDto>> GetActiveTasksAsync(int bossBranchId);
        Task<IEnumerable<EmployeeWorkloadDto>> GetEmployeeWorkloadAsync(int bossBranchId);
        Task<IEnumerable<AvailableEmployeeDto>> GetAvailableEmployeesAsync(int bossBranchId);

        /// <summary>
        /// Получить все позиции филиала для древовидного селектора
        /// </summary>
        Task<IEnumerable<PositionCellDto>> GetPositionsAsync(int bossBranchId);

        /// <summary>
        /// Получить список заказов, доступных для сборки (в статусе Processing и без назначений)
        /// </summary>
        Task<IEnumerable<AvailableOrderDto>> GetAvailableOrdersAsync(int bossBranchId);
    }
}
