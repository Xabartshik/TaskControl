using LinqToDB.Mapping;
using System;

namespace TaskControl.TaskModule.DataAccess.Models;

[Table("order_assembly_assignments")]
public class OrderAssemblyAssignmentModel
{
    [PrimaryKey, Identity]
    [Column("id")]
    public int Id { get; set; }

    [Column("task_id"), NotNull]
    public int TaskId { get; set; }

    [Column("order_id"), NotNull]
    public int OrderId { get; set; }

    [Column("assigned_to_user_id"), NotNull]
    public int AssignedToUserId { get; set; }

    [Column("branch_id"), NotNull]
    public int BranchId { get; set; }

    [Column("status"), NotNull]
    public int Status { get; set; }

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("assigned_at"), NotNull]
    public DateTime AssignedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
}

[Table("order_assembly_lines")]
public class OrderAssemblyLineModel
{
    [PrimaryKey, Identity]
    [Column("id")]
    public int Id { get; set; }

    [Column("order_assembly_assignment_id"), NotNull]
    public int OrderAssemblyAssignmentId { get; set; }

    [Column("item_position_id"), NotNull]
    public int ItemPositionId { get; set; }

    [Column("source_position_id"), NotNull]
    public int SourcePositionId { get; set; }

    [Column("target_position_id"), NotNull]
    public int TargetPositionId { get; set; }

    [Column("quantity"), NotNull]
    public int Quantity { get; set; }

    [Column("picked_quantity"), NotNull]
    public int PickedQuantity { get; set; }

    [Column("status"), NotNull]
    public int Status { get; set; }
}
