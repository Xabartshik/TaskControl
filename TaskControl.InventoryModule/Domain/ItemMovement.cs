using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InventoryModule.Domain
{
    /// <summary>
    /// Запись о перемещении товара между позициями
    /// </summary>
    public class ItemMovement
    {
        [Key]
        public int Id { get; set; }

        public int? SourceItemPositionId { get; set; }

        public int? SourceBranchId { get; set; }

        public int? DestinationBranchId { get; set; }

        public int? DestinationPositionId { get; set; }

        [Required]
        [Range(0.001, double.MaxValue)]
        public decimal Quantity { get; set; }

    }
}
