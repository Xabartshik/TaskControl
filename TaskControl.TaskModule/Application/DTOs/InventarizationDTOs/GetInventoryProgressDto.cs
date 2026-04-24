using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// DTO для получения текущего прогресса инвентаризации
    /// </summary>
    public class GetInventoryProgressDto
    {
        public int AssignmentId { get; set; }
        public InventoryStatisticsDto? CurrentStatistics { get; set; }
        public List<InventoryAssignmentLineDto> RemainingItems { get; set; } = new();
        public InventoryAssignmentStatus Status { get; set; }
        public double TimeSpentMinutes { get; set; }
        public double EstimatedTimeRemainingMinutes { get; set; }
    }
}
