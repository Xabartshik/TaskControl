using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.Application.DTOs
{
    public record PositionDto
    {
        public int PositionCellId { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
        public int BranchId { get; init; }

        [Required]
        [RegularExpression("^(Active|Inactive|Maintenance)$")]
        public string Status { get; init; } = "Active";

        [StringLength(10)]
        public string? ZoneCode { get; init; }

        [Required]
        [StringLength(30)]
        public string FirstLevelStorageType { get; init; }

        [StringLength(20)]
        public string? FLSNumber { get; init; }

        [StringLength(30)]
        public string? SecondLevelStorage { get; init; }

        [StringLength(30)]
        public string? ThirdLevelStorage { get; init; }

        [Range(0.01, 20)]
        public decimal Length { get; init; }

        [Range(0.01, 20)]
        public decimal Width { get; init; }

        [Range(0.01, 20)]
        public decimal Height { get; init; }

        public static PositionCell FromDto(PositionDto dto) => new()
        {
            PositionId = dto.PositionCellId,
            BranchId = dto.BranchId,
            Status = dto.Status,
            ZoneCode = dto.ZoneCode,
            FirstLevelStorageType = dto.FirstLevelStorageType,
            FLSNumber = dto.FLSNumber,
            SecondLevelStorage = dto.SecondLevelStorage,
            ThirdLevelStorage = dto.ThirdLevelStorage,
            Length = dto.Length,
            Width = dto.Width,
            Height = dto.Height
        };

        public static PositionDto ToDto(PositionCell entity) => new()
        {
            PositionCellId = entity.PositionId,
            BranchId = entity.BranchId,
            Status = entity.Status,
            ZoneCode = entity.ZoneCode,
            FirstLevelStorageType = entity.FirstLevelStorageType,
            FLSNumber = entity.FLSNumber,
            SecondLevelStorage = entity.SecondLevelStorage,
            ThirdLevelStorage = entity.ThirdLevelStorage,
            Length = entity.Length,
            Width = entity.Width,
            Height = entity.Height
        };
    }
}
