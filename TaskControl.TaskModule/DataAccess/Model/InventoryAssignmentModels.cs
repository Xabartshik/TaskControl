using LinqToDB.Mapping;

namespace TaskControl.TaskModule.DataAccess.Models;

[Table("inventory_assignments")]
public class InventoryAssignmentModel
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column("task_id"), NotNull]
    public int TaskId { get; set; }

    [Column("assigned_to_user_id"), NotNull]
    public int AssignedToUserId { get; set; }

    [Column("branch_id"), NotNull]
    public int BranchId { get; set; }

    [Column("zone_code")]
    public string ZoneCode { get; set; }

    [Column("status"), NotNull]
    public int Status { get; set; }

    [Column("assigned_at"), NotNull]
    public DateTime AssignedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
}

[Table("inventory_assignment_lines")]
public class InventoryAssignmentLineModel
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column("inventory_assignment_id"), NotNull]
    public int InventoryAssignmentId { get; set; }

    [Column("item_position_id"), NotNull]
    public int ItemPositionId { get; set; }

    [Column("position_id"), NotNull]
    public int PositionId { get; set; }

    [Column("expected_quantity"), NotNull]
    public int ExpectedQuantity { get; set; }

    [Column("actual_quantity")]
    public int? ActualQuantity { get; set; }

    [Column("zone_code"), NotNull]
    public string ZoneCode { get; set; }

    [Column("first_level_storage_type"), NotNull]
    public string FirstLevelStorageType { get; set; }

    [Column("fls_number"), NotNull]
    public string FLSNumber { get; set; }

    [Column("second_level_storage")]
    public string SecondLevelStorage { get; set; }

    [Column("third_level_storage")]
    public string ThirdLevelStorage { get; set; }
}
