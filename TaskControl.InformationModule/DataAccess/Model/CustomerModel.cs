using LinqToDB.Mapping;
using System;

namespace TaskControl.InformationModule.DataAccess.Model
{
    [Table("customers")]
    public class CustomerModel
    {
        [Column("customer_id")][PrimaryKey][Identity] public int CustomerId { get; set; }
        [Column("first_name")][NotNull] public string FirstName { get; set; } = null!;
        [Column("last_name")][NotNull] public string LastName { get; set; } = null!;
        [Column("phone")][NotNull] public string? Phone { get; set; }
        [Column("email")] public string? Email { get; set; }
        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}