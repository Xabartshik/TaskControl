using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.OrderModule.Domain;

namespace TaskControl.OrderModule.Application.DTOs
{
    public record OrderPositionDto
    {
        public int UniqueID { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
        public int OrderId { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ItemPositionId { get; init; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; init; }

        public static OrderPosition FromDto(OrderPositionDto dto) => new()
        {
            UniqueID = dto.UniqueID,
            OrderId = dto.OrderId,
            ItemPositionId = dto.ItemPositionId,
            Quantity = dto.Quantity
        };

        public static OrderPositionDto ToDto(OrderPosition entity) => new()
        {
            UniqueID = entity.UniqueID,
            OrderId = entity.OrderId,
            ItemPositionId = entity.ItemPositionId,
            Quantity = entity.Quantity
        };
    }
}
