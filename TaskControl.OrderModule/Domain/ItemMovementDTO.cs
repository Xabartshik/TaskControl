using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.OrderModule.Domain
{
    /// <summary>
    /// Запись о перемещении товара между позициями
    /// </summary>
    public class ItemMovement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SourceItemPositionId { get; set; }


        [Required]
        public int SourceBranchId { get; set; }

        [Required]
        public int DestinationBranchId { get; set; }

        public int DestinationPositionId { get; set; }

        [Required]
        [Range(0.001, double.MaxValue)]
        public decimal Quantity { get; set; }



        [StringLength(50)]
        public string? Status { get; set; }
    }
}
