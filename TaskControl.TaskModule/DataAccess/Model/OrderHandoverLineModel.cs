using LinqToDB.Mapping;

namespace TaskControl.TaskModule.DataAccess.Models
{
    [Table("order_handover_lines")]
    public class OrderHandoverLineModel
    {
        [Column("id"), PrimaryKey, Identity] public int Id { get; set; }
        [Column("order_handover_assignment_id"), NotNull] public int OrderHandoverAssignmentId { get; set; }
        [Column("order_position_id"), NotNull] public int OrderPositionId { get; set; }
        [Column("item_position_id")] public int? ItemPositionId { get; set; }
        [Column("quantity"), NotNull] public int Quantity { get; set; }
        [Column("scanned_quantity"), NotNull] public int ScannedQuantity { get; set; }
    }
}