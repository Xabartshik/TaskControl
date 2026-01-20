using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// DTO для детальной информации о назначении инвентаризации
    /// </summary>
    public class InventoryAssignmentDetailedDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int AssignedToUserId { get; set; }
        public string? UserName { get; set; }
        public int BranchId { get; set; }
        public string? ZoneCode { get; set; }
        public InventoryAssignmentStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<InventoryAssignmentLineDto> Lines { get; set; } = new();
    }

}
