using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // В метрах для API
        [Range(0.01, 10)]
        public double LengthMeters { get; init; }

        [Range(0.01, 10)]
        public double WidthMeters { get; init; }

        [Range(0.01, 10)]
        public double HeightMeters { get; init; }
    }
}
