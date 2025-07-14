using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace TaskControl.InventoryModule.DataAccess.Model
{
    [Table("item_positions")]
    public class ItemPositionModel
    {
        [Column("id")][PrimaryKey][Identity] public int Id { get; set; }
        [Column("item_id")][NotNull] public int ItemId { get; set; }
        [Column("position_id")][NotNull] public int PositionId { get; set; }
        [Column("quantity")][NotNull] public int Quantity { get; set; }
        [Column("length")] public Length Length { get; set; }
        [Column("width")] public Length Width { get; set; }
        [Column("height")] public Length Height { get; set; }
        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}
