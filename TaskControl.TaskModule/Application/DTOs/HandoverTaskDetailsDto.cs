using System;
using System.Collections.Generic;

namespace TaskControl.TaskModule.Application.DTOs
{
    public class HandoverTaskDetailsDto
    {
        public int AssignmentId { get; set; }
        public int TaskId { get; set; }
        public string TaskNumber { get; set; }
        public int OrderId { get; set; }

        // "ToCustomer" (Самовывоз) или "ToCourier" (Передача курьеру)
        public string HandoverType { get; set; }

        public int Status { get; set; }

        // Для кооператива (например, грузчик везет рохлю)
        public bool IsCooperative { get; set; }
        public string PartnerName { get; set; }
        public int? PartnerStatus { get; set; }
        public string TargetName { get; set; } // Имя курьера или "Покупатель"

        public List<HandoverItemDto> ItemsToScan { get; set; } = new();
    }

    public class HandoverItemDto
    {
        public int LineId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }

        // Откуда забирать товар
        public string SourceCellCode { get; set; }

        public int Quantity { get; set; }
        public int ScannedQuantity { get; set; }
        public decimal Price { get; set; }

    }
}