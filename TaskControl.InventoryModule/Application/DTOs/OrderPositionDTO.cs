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
        public int Id { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
        public int OrderId { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ItemPositionId { get; init; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; init; }

        public static OrderPosition FromDto(OrderPositionDTO dto) => new()
        {
            UniqueID = dto.Id,
            OrderId = dto.OrderId,
            ItemPositionId = dto.ItemPositionId,
            Quantity = dto.Quantity
        };

        public static OrderPositionDTO ToDto(OrderPosition entity) => new()
        {
            Id = entity.UniqueID,
            OrderId = entity.OrderId,
            ItemPositionId = entity.ItemPositionId,
            Quantity = entity.Quantity
        };
    }
}
