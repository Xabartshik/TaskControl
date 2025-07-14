using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.OrderModule.Domain;

namespace TaskControl.OrderModule.Application.DTOs
{
    public record OrderPositionDTO
    {
        public int UniqueId { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
        public int OrderId { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ItemPositionId { get; init; }

        [Required]
        public int Quantity { get; init; }

        public static OrderPosition FromDto(OrderPositionDTO dto) => new()
        {
            UniqueId = dto.UniqueId,
            OrderId = dto.OrderId,
            ItemPositionId = dto.ItemPositionId,
            Quantity = dto.Quantity
        };

        public static OrderPositionDTO ToDto(OrderPosition entity) => new()
        {
            UniqueId = entity.UniqueId,
            OrderId = entity.OrderId,
            ItemPositionId = entity.ItemPositionId,
            Quantity = entity.Quantity
        };
    }
}
