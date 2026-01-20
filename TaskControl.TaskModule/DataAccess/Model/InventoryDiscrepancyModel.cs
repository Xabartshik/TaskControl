using LinqToDB.Mapping;

namespace TaskControl.TaskModule.DataAccess.Models;

[Table("inventory_discrepancies")]
public class InventoryDiscrepancyModel
{
    [PrimaryKey, Identity]
    [Column("id")]
    public int Id { get; set; }

    [Column("inventory_assignment_line_id"), NotNull]
    public int InventoryAssignmentLineId { get; set; }

    [Column("item_position_id"), NotNull]
    public int ItemPositionId { get; set; }

    [Column("expected_quantity"), NotNull]
    public int ExpectedQuantity { get; set; }

    [Column("actual_quantity"), NotNull]
    public int ActualQuantity { get; set; }

    [Column("type"), NotNull]
    public int Type { get; set; }

    [Column("note")]
    public string Note { get; set; }

    [Column("identified_at"), NotNull]
    public DateTime IdentifiedAt { get; set; }

    [Column("resolution_status"), NotNull]
    public int ResolutionStatus { get; set; }

    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }
}
