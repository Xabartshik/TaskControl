using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Application.DTOs;

namespace TaskControl.ReportsModule.Application.Interface
{
    public interface IAnalyticsQueryProvider
    {
        Task<IEnumerable<WorkerEfficiencyResultDto>> GetWorkerEfficiencyAsync(AnalyticsQueryDto query);
        Task<IEnumerable<BranchSummaryResultDto>> GetBranchSummaryAsync(AnalyticsQueryDto query);
        Task<IEnumerable<DetailedTaskReportDto>> GetDetailedBranchReportAsync(int branchId, DateTime? start, DateTime? end);
        Task<IEnumerable<DetailedTaskReportDto>> GetDetailedWorkerReportAsync(int workerId, DateTime? start, DateTime? end);
        Task<IEnumerable<DetailedTaskReportDto>> GetCompletedInventoriesAsync(DateTime? start, DateTime? end);
        Task<IEnumerable<TaskGroupReportDto>> GetGroupedBranchReportAsync(int branchId, DateTime? start, DateTime? end);
        }
}