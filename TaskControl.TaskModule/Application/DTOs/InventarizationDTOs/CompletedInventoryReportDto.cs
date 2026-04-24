using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// DTO для отчёта о завершённой инвентаризации
    /// </summary>
    public class CompletedInventoryReportDto
    {
        public int AssignmentId { get; set; }
        public int UserId { get; set; }
        public int BranchId { get; set; }
        public string ZoneCode { get; set; }
        public DateTime CompletedAt { get; set; }
        public long DurationSeconds { get; set; }
        public int TotalPositions { get; set; }
        public int DiscrepancyCount { get; set; }
        public decimal AccuracyPercentage { get; set; }
    }
}
