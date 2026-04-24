using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.DTOs.InventorizationDTOs
{
    /// <summary>
    /// DTO для полного отчёта по расхождениям инвентаризации
    /// </summary>
    public class DiscrepancyReportDto
    {
        public int InventoryAssignmentId { get; set; }
        public int TotalDiscrepancies { get; set; }
        public int SurplusCount { get; set; }
        public int ShortageCount { get; set; }
        public decimal DiscrepancyPercentage { get; set; }
        public List<DiscrepancyDto> Discrepancies { get; set; } = new();
    }

}
