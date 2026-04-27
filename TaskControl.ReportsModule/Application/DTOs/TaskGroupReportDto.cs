using System.Collections.Generic;

namespace TaskControl.ReportsModule.Application.DTOs
{
    public class TaskGroupReportDto
    {
        public string GroupName { get; set; } // Например, "Инвентаризация от 24.04.2026"
        public int TotalWorkers { get; set; }
        public int TotalItems { get; set; }
        public int TotalDurationSeconds { get; set; }
        public int TotalDiscrepancies { get; set; }

        // Список сотрудников, выполнявших эту задачу
        public List<DetailedTaskReportDto> Workers { get; set; } = new();
    }
}