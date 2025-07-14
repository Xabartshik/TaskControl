using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.OrderModule.Domain;

namespace TaskControl.InventoryModule.Application.DTOs
{
    public record ItemStatusDTO
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Id { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ItemPositionId { get; init; }

        [Required]
        [RegularExpression("^(Available|Reserved|Shipped|Defective)$")]
        public string Status { get; init; }

        [Required] 
        public DateTime StatusDate { get; set; }

        [Required]
        public int Quantity { get; set; }

        public static ItemStatus FromDto(ItemStatusDTO dto) => new()
        {
            Id = dto.Id,
            ItemPositionId = dto.ItemPositionId,
            Status = dto.Status,
            StatusDate = dto.StatusDate,
            Quantity = dto.Quantity
        };

        public static ItemStatusDTO ToDto(ItemStatus entity) => new()
        {
            Id = entity.Id,
            ItemPositionId = entity.ItemPositionId,
            Status = entity.Status,
            StatusDate = entity.StatusDate,
            Quantity = entity.Quantity
        };
    }
}
