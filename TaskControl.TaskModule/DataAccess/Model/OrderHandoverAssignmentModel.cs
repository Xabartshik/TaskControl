using LinqToDB.Mapping;
using System;

namespace TaskControl.TaskModule.DataAccess.Models
{
    [Table("order_handover_assignments")]
    public class OrderHandoverAssignmentModel
    {
        [Column("id"), PrimaryKey, Identity] public int Id { get; set; }
        [Column("task_id"), NotNull] public int TaskId { get; set; }
        [Column("order_id"), NotNull] public int OrderId { get; set; }
        [Column("assigned_to_user_id")] public int? AssignedToUserId { get; set; }
        [Column("target_courier_id")] public int? TargetCourierId { get; set; }
        [Column("role")] public string Role { get; set; } = "Main";
        [Column("status"), NotNull] public int Status { get; set; }
        [Column("handover_type"), NotNull] public string HandoverType { get; set; } // "ToCustomer" или "ToCourier"
        [Column("assigned_at"), NotNull] public DateTime AssignedAt { get; set; }
        [Column("started_at")] public DateTime? StartedAt { get; set; }
        [Column("completed_at")] public DateTime? CompletedAt { get; set; }
    }
}