using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// DTO для получения текущей статистики инвентаризации
    /// </summary>
    public class InventoryStatisticsDto
    {
        public int Id { get; set; }
        public int InventoryAssignmentId { get; set; }
        public int TotalPositions { get; set; }
        public int CountedPositions { get; set; }
        public decimal CompletionPercentage { get; set; }
        public int DiscrepancyCount { get; set; }
        public int SurplusCount { get; set; }
        public int ShortageCount { get; set; }
        public int TotalSurplusQuantity { get; set; }
        public int TotalShortageQuantity { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
