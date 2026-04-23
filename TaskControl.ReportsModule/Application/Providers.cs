using LinqToDB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Application.DTOs;
using TaskControl.ReportsModule.Application.Interface;
using TaskControl.ReportsModule.DataAccess.Interface;

namespace TaskControl.ReportsModule.DataAccess.Providers
{
    public class AnalyticsQueryProvider : IAnalyticsQueryProvider
    {
        private readonly IReportDataConnection _db;

        public AnalyticsQueryProvider(IReportDataConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<WorkerEfficiencyResultDto>> GetWorkerEfficiencyAsync(AnalyticsQueryDto query)
        {
            var queryable = _db.WorkerTaskEfficiency
                .Where(x => x.CompletedAt >= query.StartDate && x.CompletedAt <= query.EndDate);

            // Динамическое добавление фильтров
            if (query.WorkerId.HasValue) queryable = queryable.Where(x => x.WorkerId == query.WorkerId.Value);
            if (query.BranchId.HasValue) queryable = queryable.Where(x => x.BranchId == query.BranchId.Value);
            if (!string.IsNullOrEmpty(query.TaskCategory)) queryable = queryable.Where(x => x.TaskCategory == query.TaskCategory);

            var result = await queryable
                .GroupBy(x => x.TaskCategory)
                .Select(g => new WorkerEfficiencyResultDto
                {
                    TaskCategory = g.Key,
                    TotalTasks = g.Count(),
                    ItemsProcessed = g.Sum(x => x.ItemsProcessed),
                    AverageWaitTimeSeconds = g.Count() > 0 ? g.Sum(x => x.WaitTimeSeconds) / g.Count() : 0,
                    AverageQueueSize = g.Count() > 0 ? g.Sum(x => x.QueueSize) / g.Count() : 0,
                    AverageDurationSeconds = g.Count() > 0 ? g.Sum(x => x.TotalDurationSeconds) / g.Count() : 0,
                    DiscrepanciesFound = g.Sum(x => x.DiscrepanciesFound)
                })
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<BranchSummaryResultDto>> GetBranchSummaryAsync(AnalyticsQueryDto query)
        {
            var queryable = _db.WorkerTaskEfficiency
                .Where(x => x.CompletedAt >= query.StartDate && x.CompletedAt <= query.EndDate);

            if (query.BranchId.HasValue) queryable = queryable.Where(x => x.BranchId == query.BranchId.Value);

            var result = await queryable
                .GroupBy(x => x.BranchId)
                .Select(g => new BranchSummaryResultDto
                {
                    BranchId = g.Key,
                    TotalWorkersActive = g.Select(x => x.WorkerId).Distinct().Count(),
                    TotalTasksCompleted = g.Count(),
                    TotalItemsMoved = g.Sum(x => x.ItemsProcessed),
                    TotalDiscrepancies = g.Sum(x => x.DiscrepanciesFound)
                })
                .ToListAsync();

            return result;
        }
    }
}