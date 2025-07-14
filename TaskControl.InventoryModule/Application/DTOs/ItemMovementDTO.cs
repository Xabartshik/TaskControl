using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InventoryModule.Application.DTOs
{
    public record ItemMovementDTO
    {
        public int Id { get; init; }

        public int? SourceItemPositionId { get; init; }

        public int? DestinationPositionId { get; init; }

        public int? SourceBranchId { get; init; }

        public int? DestinationBranchId { get; init; }

        [Required]
        public int Quantity { get; init; }


    }
}
