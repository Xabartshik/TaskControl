namespace TaskControl.TaskModule.Domain;

public enum MobileUserRole
{
    Worker = 1,
    Supervisor = 2,
    Admin = 3,
    Customer = 4
}

/// <summary>
/// Пользователь мобильного приложения, привязанный к сотруднику.
/// Логин = EmployeeId, пароль хранится как хэш.
/// </summary>
public class MobileAppUser
{
    /// <summary>
    /// Идентификатор записи (из БД).
    /// </summary>
    public int Id { get;  set; }

    public int? EmployeeId { get; set; }
    public int? CustomerId { get; set; }
    public string Login { get; set; }
    /// <summary>
    /// Хэш пароля.
    /// </summary>
    public string PasswordHash { get;  set; }

    /// <summary>
    /// Роль пользователя в мобильном приложении.
    /// </summary>
    public MobileUserRole Role { get; set; }

    /// <summary>
    /// Идентификатор филиала.
    /// </summary>
    public int? BranchId { get; set; }

    /// <summary>
    /// Признак активного пользователя.
    /// Неактивный не может войти.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата последнего изменения.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    internal MobileAppUser() { }

    public MobileAppUser(
        string login,
        string passwordHash,
        MobileUserRole role,
        int? employeeId = null,
        int? customerId = null,
        int? branchId = null)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ArgumentException("Login cannot be empty.", nameof(login));

        Login = login;
        PasswordHash = passwordHash;
        Role = role;
        EmployeeId = employeeId;
        CustomerId = customerId;
        BranchId = branchId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRole(MobileUserRole role)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
