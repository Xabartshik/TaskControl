using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.OrderModule.DataAccess.Model
{
    [Table("orders")]
    public class OrderModel
    {
        [Column("order_id"), PrimaryKey, Identity] public int Id { get; set; }
        [Column("customer_id"), NotNull] public int CustomerId { get; set; }
        [Column("branch_id"), NotNull] public int BranchId { get; set; }
        [Column("delivery_date")] public DateTime? DeliveryDate { get; set; }
        [Column("type"), NotNull] public string Type { get; set; }
        [Column("status"), NotNull] public string Status { get; set; }
        [Column("created_at"), NotNull] public DateTime CreatedAt { get; set; }
    }
}
