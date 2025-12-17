using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// DTO для строки назначения инвентаризации (товар для учёта)
    /// </summary>
    public class InventoryAssignmentLineDto
    {
        public int Id { get; set; }
        public int InventoryAssignmentId { get; set; }
        public int ItemPositionId { get; set; }
        public int ExpectedQuantity { get; set; }
        public int? ActualQuantity { get; set; }
        public int Variance => ActualQuantity.HasValue ? ActualQuantity.Value - ExpectedQuantity : 0;
        public string? ZoneCode { get; set; }
        public string? FirstLevelStorageType { get; set; }
        public int? FlsNumber { get; set; }
    }
}
