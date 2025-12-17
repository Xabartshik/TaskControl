using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// DTO для отчёта производительности работника
    /// </summary>
    public class WorkerPerformanceReportDto
    {
        public int UserId { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int CompletedInventories { get; set; }
        public int TotalItemsCount { get; set; }
        public decimal AverageAccuracy { get; set; }
        public long AverageDurationSeconds { get; set; }
    }
}
