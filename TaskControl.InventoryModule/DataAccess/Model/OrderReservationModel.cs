using LinqToDB.Mapping;
using System;

namespace TaskControl.InventoryModule.DataAccess.Model
{
    [Table("order_reservations")]
    public class OrderReservationModel
    {
        [Column("id")][PrimaryKey][Identity] public int Id { get; set; }
        [Column("order_position_id")][NotNull] public int OrderPositionId { get; set; }
        [Column("item_position_id")][NotNull] public int ItemPositionId { get; set; }
        [Column("quantity")][NotNull] public int Quantity { get; set; }
        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}
