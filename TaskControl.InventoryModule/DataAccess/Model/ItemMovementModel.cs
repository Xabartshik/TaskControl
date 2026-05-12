using LinqToDB.Mapping;


namespace TaskControl.InventoryModule.DataAccess.Model
{
    [Table("item_movements")]
    public class ItemMovementModel
    {
        [Column("id")][PrimaryKey][Identity] public int Id { get; set; }

        [Column("item_id")][NotNull] public int ItemId { get; set; } // НОВОЕ

        [Column("source_position_id")] public int? SourcePositionId { get; set; } // ИЗМЕНЕНО
        [Column("destination_position_id")] public int? DestinationPositionId { get; set; }

        [Column("source_branch_id")] public int? SourceBranchId { get; set; }
        [Column("destination_branch_id")] public int? DestinationBranchId { get; set; }

        [Column("quantity")][NotNull] public int Quantity { get; set; }

        [Column("worker_id")] public int? WorkerId { get; set; } // НОВОЕ (кто переместил)
        [Column("task_id")] public int? TaskId { get; set; } // НОВОЕ (в рамках какой задачи)

        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}