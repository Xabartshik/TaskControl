using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InventoryModule.Application.DTOs
{
    public record ItemStatusDTO
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ItemPositionId { get; init; }

        [Required]
        [RegularExpression("^(Available|Reserved|Shipped|Defective)$")]
        public string Status { get; init; }

        [Required]
        public int Quantity { get; set; }
    }
}
