using LinqToDB.Mapping;
using System;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Model
{
    [Table("base_tasks")]
    public class BaseTaskModel
    {
        [Column("task_id"), PrimaryKey, Identity]
        public int TaskId { get; set; }

        [Column("title"), NotNull]
        public string Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("branch_id"), NotNull]
        public int BranchId { get; set; }

        [Column("type"), NotNull]
        public string Type { get; set; }

        [Column("created_at"), NotNull]
        public DateTime CreatedAt { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        // Храним как INT в БД, но работаем как с enum в коде
        [Column("status"), NotNull]
        public int Status { get; set; }

        [Column("priority"), NotNull]
        public int Priority { get; set; }
    }
}
