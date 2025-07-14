using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InventoryModule.DataAccess.Model
{
    [Table("item_statuses")]
    public class ItemStatusModel
    {
        [Column("id")][PrimaryKey][Identity] public int Id { get; set; }
        [Column("item_position_id")][NotNull] public int ItemPositionId { get; set; }
        [Column("status")][NotNull] public string Status { get; set; }
        [Column("status_date")][NotNull] public DateTime StatusDate { get; set; }
        [Column("quantity")][NotNull] public int Quantity { get; set; }
    }
}
