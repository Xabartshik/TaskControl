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
    public record PositionCellDto
    {
        public int PositionId { get; init; }

        [Required]
        public int BranchId { get; init; }

        [Required]
        [RegularExpression("^(Active|Inactive|Maintenance)$")]
        public string Status { get; init; } = "Active";

        [Required]
        [StringLength(10)]
        public string ZoneCode { get; init; }

        [Required]
        [StringLength(30)]
        public string FirstLevelStorageType { get; init; }

        [Required]
        [StringLength(20)]
        public string FLSNumber { get; init; }


        [StringLength(30)]
        public string? SecondLevelStorage { get; init; }

        [StringLength(30)]
        public string? ThirdLevelStorage { get; init; }

        public Length Length { get; init; }

        public Length Width { get; init; }

        public Length Height { get; init; }

        public static PositionCell FromDto(PositionCellDto dto) => new()
        {
            PositionId = dto.PositionId,
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

        public static PositionCellDto ToDto(PositionCell entity) => new()
        {
            PositionId = entity.PositionId,
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
