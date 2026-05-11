using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Application.DTOs;

namespace TaskControl.ReportsModule.Application.Interface
{
    public interface IOrderAnalyticsService
    {
        // Методы для получения сырых данных (JSON)
        Task<List<OrderLeadTimeDto>> GetOrderLeadTimesAsync(AnalyticsFilterDto filter);
        Task<List<TopItemDto>> GetTopItemsAsync(AnalyticsFilterDto filter);
        Task<List<EmployeeKpiDto>> GetEmployeeKpiAsync(AnalyticsFilterDto filter);
        Task<List<BranchSummaryDto>> GetBranchSummaryAsync(AnalyticsFilterDto filter);

        Task<List<OrderDetailedDto>> GetDetailedOrdersAsync(AnalyticsFilterDto filter);
        Task<List<EmployeeTaskDetailDto>> GetEmployeeTasksDetailAsync(int employeeId, AnalyticsFilterDto filter);

        // Методы генерации PDF (новые)
        Task<byte[]> GenerateDetailedOrdersPdfAsync(AnalyticsFilterDto filter);
        Task<byte[]> GenerateTopItemsPdfAsync(AnalyticsFilterDto filter);
        Task<List<EmployeeFullReportDto>> GetEmployeeFullReportsAsync(AnalyticsFilterDto filter);
        Task<OrderDashboardReportDto> GetOrderDashboardAsync(AnalyticsFilterDto filter);

        // Методы для получения готовых файлов (PDF)
        Task<byte[]> GenerateEmployeeFullReportPdfAsync(AnalyticsFilterDto filter);
        Task<byte[]> GenerateOrderDashboardPdfAsync(AnalyticsFilterDto filter);
    }
}