using LinqToDB.Mapping;

namespace TaskControl.TaskModule.DataAccess.Models;

[Table("mobile_app_users")]
public class MobileAppUserModel
{
    [PrimaryKey, Identity]
    [Column("id")]
    public int Id { get; set; }

    [Column("login"), NotNull]
    public string Login { get; set; } = null!;

    [Column("employee_id")]
    public int? EmployeeId { get; set; }

    [Column("customer_id")]
    public int? CustomerId { get; set; }

    [Column("password_hash"), NotNull]
    public string PasswordHash { get; set; } = null!;

    [Column("role"), NotNull]
    public int Role { get; set; } // Мапится на int в БД

    [Column("is_active"), NotNull]
    public bool IsActive { get; set; }

    [Column("created_at"), NotNull]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("branch_id")]
    public int? BranchId { get; set; }
}