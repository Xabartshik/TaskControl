using LinqToDB.Mapping;

namespace TaskControl.TaskModule.DataAccess.Models;

[Table("inventory_statistics")]
public class InventoryStatisticsModel
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column("inventory_assignment_id"), NotNull]
    public int InventoryAssignmentId { get; set; }

    [Column("total_positions"), NotNull]
    public int TotalPositions { get; set; }

    [Column("counted_positions"), NotNull]
    public int CountedPositions { get; set; }

    [Column("discrepancy_count"), NotNull]
    public int DiscrepancyCount { get; set; }

    [Column("surplus_count"), NotNull]
    public int SurplusCount { get; set; }

    [Column("shortage_count"), NotNull]
    public int ShortageCount { get; set; }

    [Column("total_surplus_quantity"), NotNull]
    public int TotalSurplusQuantity { get; set; }

    [Column("total_shortage_quantity"), NotNull]
    public int TotalShortageQuantity { get; set; }

    [Column("started_at"), NotNull]
    public DateTime StartedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
}
