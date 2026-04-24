using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{

    public class InventoryTaskDetailsDto
    {
        public int TaskId { get; set; }
        public string ZoneCode { get; set; } = null!;
        public List<InventoryItemDto> Items { get; set; } = new();
        public int TotalExpectedCount { get; set; }
        public DateTime InitiatedAt { get; set; }
    }

    public class InventoryItemDto
    {
        public int ItemId { get; set; }
        public int? LineId { get; set; }
        public string ItemName { get; set; } = null!;
        public string PositionCode { get; set; } = null!;
        public int PositionId { get; set; }
        public int ExpectedQuantity { get; set; }
        public double Weight { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Status { get; set; } = "Available"; 
    }

}
