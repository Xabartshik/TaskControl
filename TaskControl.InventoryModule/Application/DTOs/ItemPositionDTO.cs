using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.Domain;
using UnitsNet;

namespace TaskControl.InventoryModule.Application.DTOs
{
    public record ItemPositionDto
    {
        public int Id { get; init; }


        [Required]
        public int ItemId { get; init; }

        [Required]
        public int PositionId { get; init; }

        [Required]
        public int Quantity { get; init; }



        public static ItemPosition FromDto(ItemPositionDto dto) => new()
        {
            Id = dto.Id,
            ItemId = dto.ItemId,
            PositionId = dto.PositionId,
            Quantity = dto.Quantity
        };

        public static ItemPositionDto ToDto(ItemPosition entity) => new()
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            PositionId = entity.PositionId,
            Quantity = entity.Quantity
        };
    }
}
