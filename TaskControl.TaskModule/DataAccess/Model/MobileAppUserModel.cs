using LinqToDB.Mapping;

namespace TaskControl.TaskModule.DataAccess.Models;

[Table("mobile_app_users")]
public class MobileAppUserModel
{
    [PrimaryKey, Identity]
    [Column("id")]
    public int Id { get; set; }

    [Column("employee_id"), NotNull]
    public int EmployeeId { get; set; }

    [Column("password_hash"), NotNull]
    public string PasswordHash { get; set; } = null!;

    [Column("role"), NotNull]
    public string Role { get; set; } = null!;

    [Column("is_active"), NotNull]
    public bool IsActive { get; set; }

    [Column("created_at"), NotNull]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
