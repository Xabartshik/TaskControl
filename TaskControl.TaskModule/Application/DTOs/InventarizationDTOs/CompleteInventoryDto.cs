using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// DTO для завершения инвентаризации с финальным отчётом
    /// </summary>
    public class CompleteInventoryDto
    {
        public int InventoryAssignmentId { get; set; }
        public InventoryStatisticsDto? Statistics { get; set; }
        public DiscrepancyReportDto? DiscrepancyReport { get; set; }
        public DateTime CompletedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
