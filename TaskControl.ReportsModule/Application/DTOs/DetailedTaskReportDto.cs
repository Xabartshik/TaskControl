using System;

namespace TaskControl.ReportsModule.Application.DTOs
{
    public class DetailedTaskReportDto
    {
        public int TaskId { get; set; }
        public string TaskCategory { get; set; } 
        public string TaskCategoryDisplayName { get; set; }
        public int WorkerId { get; set; }
        public string WorkerFullName { get; set; }
        public DateTime CompletedAt { get; set; }
        public int DurationSeconds { get; set; }
        public int WaitTimeSeconds { get; set; }
        public int ItemsProcessed { get; set; }
        public int Discrepancies { get; set; }
        public int QueueSize { get; set; }
    }
}