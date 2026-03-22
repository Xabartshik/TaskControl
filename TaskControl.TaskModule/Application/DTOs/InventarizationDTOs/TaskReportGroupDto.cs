using System;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    public class TaskReportGroupDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AssignmentsProgress { get; set; }
    }
}
