using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InventoryModule.DataAccess.Model
{
    [Table("order_positions")]
    public class OrderPositionModel
    {
        [Column("unique_id")][PrimaryKey][Identity] public int UniqueId { get; set; }
        [Column("order_id")][NotNull] public int OrderId { get; set; }
        [Column("item_position_id")][NotNull] public int ItemPositionId { get; set; }
        [Column("quantity")][NotNull] public int Quantity { get; set; }
        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}
