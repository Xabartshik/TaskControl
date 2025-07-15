using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace TaskControl.InventoryModule.DataAccess.Model
{
    [Table("positions")]
    public class PositionModel
    {
        [Column("position_id")][PrimaryKey][Identity] public int PositionId { get; set; }
        [Column("branch_id")][NotNull] public int BranchId { get; set; }
        [Column("status")][NotNull] public string Status { get; set; }
        [Column("zone_code")][NotNull] public string ZoneCode { get; set; }
        [Column("first_level_storage_type")][NotNull] public string FirstLevelStorageType { get; set; }
        [Column("fls_number")][NotNull] public string FLSNumber { get; set; }
        [Column("second_level_storage")] public string? SecondLevelStorage { get; set; }
        [Column("third_level_storage")] public string? ThirdLevelStorage { get; set; }
        [Column("length")] public double Length { get; set; }
        [Column("width")] public double Width { get; set; }
        [Column("height")] public double Height { get; set; }
        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}
