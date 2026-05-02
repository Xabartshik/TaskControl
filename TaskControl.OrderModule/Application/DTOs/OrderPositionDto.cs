using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.OrderModule.Application.DTOs
{
    public record OrderPositionDto
    {
        public int UniqueId { get; init; }

        [Required]
        public int OrderId { get; init; }

        [Required]
        public int ItemId { get; init; }

        [Required]
        public int Quantity { get; init; }

        [Required]
        public decimal Price { get; set; }
        public string? ItemName { get; set; }

        public static OrderPosition FromDto(OrderPositionDto dto) => new()
        {
            UniqueId = dto.UniqueId,
            OrderId = dto.OrderId,
            ItemId = dto.ItemId,
            Quantity = dto.Quantity,
            Price = dto.Price
        };

        public static OrderPositionDto ToDto(OrderPosition entity) => new()
        {
            UniqueId = entity.UniqueId,
            OrderId = entity.OrderId,
            ItemId = entity.ItemId,
            Quantity = entity.Quantity,
            Price = entity.Price
        };
    }
}
