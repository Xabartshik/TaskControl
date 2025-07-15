using LinqToDB.Mapping;


namespace TaskControl.TaskModule.DataAccess.Model
{
    [Table("active_tasks")]
    public class ActiveTaskModel
    {
        [Column("task_id"), PrimaryKey, Identity] public int TaskId { get; set; }
        [Column("branch_id"), NotNull] public int BranchId { get; set; }
        [Column("type"), NotNull] public string Type { get; set; }
        [Column("created_at"), NotNull] public DateTime CreatedAt { get; set; }
        [Column("completed_at")] public DateTime? CompletedAt { get; set; }
        [Column("status"), NotNull] public string Status { get; set; }
        [Column("json_params")] public string? JsonParams { get; set; }
    }
}
