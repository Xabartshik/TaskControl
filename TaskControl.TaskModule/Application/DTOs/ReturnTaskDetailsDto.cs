using System.Collections.Generic;

namespace TaskControl.TaskModule.Application.DTOs
{
    public class ReturnTaskDetailsDto
    {
        public int AssignmentId { get; set; }
        public int TaskId { get; set; }
        public string TaskNumber { get; set; }

        // 0=New, 1=InProgress, 2=Completed
        public int Status { get; set; }

        // "Main" или "Helper"
        public string Role { get; set; }

        // Для кооператива (тяжелые грузы)
        public bool IsCooperative { get; set; }
        public string? PartnerName { get; set; }
        public int? PartnerStatus { get; set; }

        public List<ReturnItemDto> ItemsToScan { get; set; } = new();
    }

    public class ReturnItemDto
    {
        public int LineId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }

        // Откуда забирать (Багажник курьера или зона выдачи Pick-up)
        public string SourceCellCode { get; set; }

        // Куда положить (Стеллаж на складе)
        public string? TargetCellCode { get; set; }

        public int Quantity { get; set; }
        public int ScannedQuantity { get; set; }
    }
}