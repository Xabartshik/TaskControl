using System;
using LinqToDB.Mapping;

namespace TaskControl.TaskModule.DataAccess.Models
{
    [Table("return_assignments")]
    public class ReturnAssignmentModel
    {
        [Column("id"), PrimaryKey, Identity] public int Id { get; set; }
        [Column("task_id"), NotNull] public int TaskId { get; set; }
        [Column("assigned_to_user_id")] public int? AssignedToUserId { get; set; }
        [Column("branch_id"), NotNull] public int BranchId { get; set; }

        [Column("status"), NotNull] public int Status { get; set; } // 0=New, 1=InProgress, 2=Completed, 3=Cancelled

        // Поддержка тяжелых грузов (кооперативные задачи)
        [Column("role")] public string Role { get; set; } = "Main"; // "Main" или "Helper"
        [Column("complexity")] public double Complexity { get; set; } = 1.0;

        [Column("assigned_at"), NotNull] public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        [Column("started_at")] public DateTime? StartedAt { get; set; }
        [Column("completed_at")] public DateTime? CompletedAt { get; set; }
    }

    [Table("return_lines")]
    public class ReturnLineModel
    {
        [Column("id"), PrimaryKey, Identity] public int Id { get; set; }
        [Column("return_assignment_id"), NotNull] public int ReturnAssignmentId { get; set; }

        // ID товара-сироты в виртуальной ячейке курьера или зоне PICKUP
        [Column("item_position_id"), NotNull] public int ItemPositionId { get; set; }

        // Складская полка, куда система рекомендует вернуть товар
        [Column("target_position_id")] public int? TargetPositionId { get; set; }

        [Column("quantity"), NotNull] public int Quantity { get; set; }
        [Column("scanned_quantity"), NotNull] public int ScannedQuantity { get; set; }
    }
}