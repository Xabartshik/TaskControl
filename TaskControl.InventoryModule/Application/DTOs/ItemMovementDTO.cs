using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.Application.DTOs
{
    public record ItemMovementDto
    {
        public int Id { get; init; }

        public int? SourceItemPositionId { get; init; }

        public int? DestinationPositionId { get; init; }

        public int? SourceBranchId { get; init; }

        public int? DestinationBranchId { get; init; }

        [Required]
        public int Quantity { get; init; }

        public static ItemMovement FromDto(ItemMovementDto dto) => new()
        {
            Id = dto.Id,
            SourceItemPositionId = dto.SourceItemPositionId,
            DestinationPositionId = dto.DestinationPositionId,
            SourceBranchId = dto.SourceBranchId,
            DestinationBranchId = dto.DestinationBranchId,
            Quantity = dto.Quantity
        };

        public static ItemMovementDto ToDto(ItemMovement entity) => new()
        {
            Id = entity.Id,
            SourceItemPositionId = entity.SourceItemPositionId,
            DestinationPositionId = entity.DestinationPositionId,
            SourceBranchId = entity.SourceBranchId,
            DestinationBranchId = entity.DestinationBranchId,
            Quantity = entity.Quantity
        };
    }


}
