using System.ComponentModel.DataAnnotations;

namespace TaskControl.InventoryModule.Domain
{
    public class ItemMovement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ItemId { get; set; } // НОВОЕ

        public int? SourcePositionId { get; set; } // ИЗМЕНЕНО
        public int? DestinationPositionId { get; set; }

        public int? SourceBranchId { get; set; }
        public int? DestinationBranchId { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int? WorkerId { get; set; } // НОВОЕ
        public int? TaskId { get; set; } // НОВОЕ
    }
}