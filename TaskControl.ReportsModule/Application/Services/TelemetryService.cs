using LinqToDB;
using System;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.ReportsModule.DataAccess.Interface;
using TaskControl.ReportsModule.DataAccess.Model;

namespace TaskControl.ReportsModule.Application.Services
{
    public class TelemetryService : ITelemetryService
    {
        private readonly IReportDataConnection _db;

        public TelemetryService(IReportDataConnection db)
        {
            _db = db;
        }

        public async Task LogTaskEventAsync(
            int workerId,
            int branchId,
            string taskCategory,
            int itemsProcessed,
            int durationSeconds,
            int discrepanciesFound = 0, 
            int waitTimeSeconds = 0,
            int queueSize = 0)
        {
            // Прямая запись готовых агрегированных метрик в таблицу проекции
            await _db.WorkerTaskEfficiency.InsertAsync(() => new WorkerTaskEfficiencyModel
            {
                WorkerId = workerId,
                BranchId = branchId,
                TaskCategory = taskCategory,
                ItemsProcessed = itemsProcessed,
                TotalDurationSeconds = durationSeconds,
                DiscrepanciesFound = discrepanciesFound,
                WaitTimeSeconds = waitTimeSeconds,
                QueueSize = queueSize,
                CompletedAt = DateTime.UtcNow
            });
        }
    }
}