using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Application.DTOs;

namespace TaskControl.ReportsModule.Application.Interface
{
    public interface IAnalyticsQueryProvider
    {
        Task<IEnumerable<WorkerEfficiencyResultDto>> GetWorkerEfficiencyAsync(AnalyticsQueryDto query);
        Task<IEnumerable<BranchSummaryResultDto>> GetBranchSummaryAsync(AnalyticsQueryDto query);
    }
}