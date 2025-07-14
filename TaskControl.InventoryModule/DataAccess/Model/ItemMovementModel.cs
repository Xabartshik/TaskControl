using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InventoryModule.DataAccess.Model
{
    [Table("item_movements")]
    public class ItemMovementModel
    {
        [Column("id")][PrimaryKey][Identity] public int Id { get; set; }
        [Column("source_item_position_id")] public int? SourceItemPositionId { get; set; }
        [Column("destination_position_id")] public int? DestinationPositionId { get; set; }
        [Column("source_branch_id")] public int? SourceBranchId { get; set; }
        [Column("destination_branch_id")] public int? DestinationBranchId { get; set; }
        [Column("quantity")][NotNull] public int Quantity { get; set; }
        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}
