using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// TODO: Пересмотреть, возможно удалить
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

    public class CompleteAssignmentDto
    {
        public int AssignmentId { get; set; }
        public int WorkerId { get; set; }
        public List<AssignmentLineUpdateDto> Lines { get; set; } = new();
    }

    public class AssignmentLineUpdateDto
    {
        public int? LineId { get; set; }           // null для неожиданных товаров
        public int ItemId { get; set; }            // Обязательно для новых товаров
        public string PositionCode { get; set; } = string.Empty;
        public int? ActualQuantity { get; set; }   // null или >= 0
    }

    public class CompleteAssignmentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public InventoryStatisticsDto? Statistics { get; set; }
        public DiscrepancyReportDto? DiscrepancyReport { get; set; }
    }

}
