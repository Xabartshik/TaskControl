using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.DataAccess.Model
{
    [Table("active_assigned_tasks")]
    public class TaskAssignationModel
    {
        [Column("id"), PrimaryKey, Identity] public int Id { get; set; }
        [Column("task_id"), NotNull] public int TaskId { get; set; }
        [Column("user_id"), NotNull] public int UserId { get; set; }
        [Column("assigned_at"), NotNull] public DateTime AssignedAt { get; set; }
    }
}
