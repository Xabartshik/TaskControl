using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Application.DTOs;

namespace TaskControl.ReportsModule.DataAccess.Interface
{
    public interface IOrderAnalyticsRepository
    {
        Task<List<OrderLeadTimeDto>> GetOrderLeadTimesAsync(AnalyticsFilterDto filter);
        Task<List<TopItemDto>> GetTopItemsAsync(AnalyticsFilterDto filter);
        Task<List<EmployeeKpiDto>> GetEmployeeKpiAsync(AnalyticsFilterDto filter);
        Task<List<BranchSummaryDto>> GetBranchSummaryAsync(AnalyticsFilterDto filter);
        Task<List<OrderDetailedDto>> GetDetailedOrdersAsync(AnalyticsFilterDto filter);
        Task<List<EmployeeTaskDetailDto>> GetEmployeeTasksDetailAsync(int employeeId, AnalyticsFilterDto filter);
        Task<List<EmployeeFullReportDto>> GetEmployeeFullReportsAsync(AnalyticsFilterDto filter);

        // Получение дашборда заказов: рекорды (вес/объем), финансовая сводка и полный реестр с составом
        Task<OrderDashboardReportDto> GetOrderDashboardAsync(AnalyticsFilterDto filter);
    }
}