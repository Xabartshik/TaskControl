using System;
using System.ComponentModel.DataAnnotations;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.Application.DTOs
{
    public record ItemMovementDto
    {
        public int Id { get; init; }

        [Required]
        public int ItemId { get; init; }

        public int? SourcePositionId { get; init; }

        public int? DestinationPositionId { get; init; }

        public int? SourceBranchId { get; init; }

        public int? DestinationBranchId { get; init; }

        [Required]
        public int Quantity { get; init; }

        public int? WorkerId { get; init; }

        public int? TaskId { get; init; }

        public static ItemMovement FromDto(ItemMovementDto dto) => new()
        {
            Id = dto.Id,
            ItemId = dto.ItemId,
            SourcePositionId = dto.SourcePositionId,
            DestinationPositionId = dto.DestinationPositionId,
            SourceBranchId = dto.SourceBranchId,
            DestinationBranchId = dto.DestinationBranchId,
            Quantity = dto.Quantity,
            WorkerId = dto.WorkerId,
            TaskId = dto.TaskId
        };

        public static ItemMovementDto ToDto(ItemMovement entity) => new()
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            SourcePositionId = entity.SourcePositionId,
            DestinationPositionId = entity.DestinationPositionId,
            SourceBranchId = entity.SourceBranchId,
            DestinationBranchId = entity.DestinationBranchId,
            Quantity = entity.Quantity,
            WorkerId = entity.WorkerId,
            TaskId = entity.TaskId
        };
    }
}