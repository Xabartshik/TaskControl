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
        [Column("order_id"), PrimaryKey, Identity] public int OrderId { get; set; }
        [Column("customer_id"), NotNull] public int CustomerId { get; set; }
        [Column("branch_id"), NotNull] public int BranchId { get; set; }
        [Column("delivery_date")] public DateTime? DeliveryDate { get; set; }

        // Поля логистики
        [Column("destination_address")] public string? DestinationAddress { get; set; }
        [Column("delivery_type"), NotNull] public string DeliveryType { get; set; }
        [Column("payment_type"), NotNull] public string PaymentType { get; set; }

        // В БД статусы храним строками для читаемости и безопасности
        [Column("status"), NotNull] public string Status { get; set; }
        [Column("created_at"), NotNull] public DateTime CreatedAt { get; set; }
        [Column("postamat_id")] public int? PostamatId { get; set; }
        [Column("total_price")] public decimal TotalPrice { get; set; }
        [Column("postamat_cell_id")] public int? PostamatCellId { get; set; }
    }
}
