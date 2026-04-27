using System.ComponentModel.DataAnnotations;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    public class WorkerStatusDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsWorking { get; set; }
        public int ActiveTaskCount { get; set; }
    }
}
