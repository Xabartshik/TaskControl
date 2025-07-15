using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.OrderModule.Domain;
using UnitsNet;

namespace TaskControl.InventoryModule.Application.DTOs
{
    public record ItemPositionDTO
    {
        public int Id { get; init; }


        [Required]
        public int ItemId { get; init; }

        [Required]
        public int PositionId { get; init; }

        [Required]
        public int Quantity { get; init; }



        public static ItemPosition FromDto(ItemPositionDTO dto) => new()
        {
            Id = dto.Id,
            ItemId = dto.ItemId,
            PositionId = dto.PositionId,
            Quantity = dto.Quantity
        };

        public static ItemPositionDTO ToDto(ItemPosition entity) => new()
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            PositionId = entity.PositionId,
            Quantity = entity.Quantity
        };
    }
}
