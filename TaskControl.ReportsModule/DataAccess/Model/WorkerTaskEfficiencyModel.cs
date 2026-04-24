using LinqToDB.Mapping;

namespace TaskControl.ReportsModule.DataAccess.Model
{
    [Table("worker_task_efficiency")]
    public class WorkerTaskEfficiencyModel
    {
        [Column("id"), PrimaryKey, Identity]
        public int Id { get; set; }

        [Column("worker_id"), NotNull]
        public int WorkerId { get; set; }

        [Column("branch_id"), NotNull]
        public int BranchId { get; set; }

        /// <summary>
        /// Категория задачи для фильтрации в отчетах
        /// </summary>
        [Column("task_category"), NotNull]
        public string TaskCategory { get; set; } = null!;

        [Column("items_processed"), NotNull]
        public int ItemsProcessed { get; set; }

        [Column("total_duration_seconds"), NotNull]
        public int TotalDurationSeconds { get; set; }
        /// <summary>
        /// Ошибки и расхождения, встреченные в ходе выполнения задачи
        /// </summary>
        [Column("discrepancies_found"), NotNull]
        public int DiscrepanciesFound { get; set; }

        [Column("completed_at"), NotNull]
        public DateTime CompletedAt { get; set; }

        [Column("wait_time_seconds"), NotNull] 
        public int WaitTimeSeconds { get; set; }

        [Column("queue_size"), NotNull] 
        public int QueueSize { get; set; }
    }
}