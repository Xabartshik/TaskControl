using System;

namespace TaskControl.ReportsModule.Application.DTOs
{
    /// <summary>
    /// Фильтр для запроса аналитики
    /// </summary>
    public class AnalyticsQueryDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? BranchId { get; set; }
        public int? WorkerId { get; set; }
        public string? TaskCategory { get; set; } // Опциональный фильтр по конкретной задаче
    }

    /// <summary>
    /// Эффективность по типу задач
    /// </summary>
    public class WorkerEfficiencyResultDto
    {
        public string TaskCategory { get; set; } = null!;
        public int TotalTasks { get; set; }
        public int ItemsProcessed { get; set; }
        public int AverageDurationSeconds { get; set; }
        public int AverageWaitTimeSeconds { get; set; }
        public int AverageQueueSize { get; set; }
        public int DiscrepanciesFound { get; set; }
    }

    /// <summary>
    /// Сводка по филиалу
    /// </summary>
    public class BranchSummaryResultDto
    {
        public int BranchId { get; set; }
        public int TotalWorkersActive { get; set; }
        public int TotalTasksCompleted { get; set; }
        public int TotalItemsMoved { get; set; }
        public int TotalDiscrepancies { get; set; }
    }
}