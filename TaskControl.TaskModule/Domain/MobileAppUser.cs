namespace TaskControl.TaskModule.Domain;

public enum MobileUserRole
{
    Worker,
    Supervisor,
    Admin
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

    /// <summary>
    /// Идентификатор сотрудника (логин в приложении).
    /// </summary>
    public int EmployeeId { get;  set; }

    /// <summary>
    /// Хэш пароля.
    /// </summary>
    public string PasswordHash { get;  set; }

    /// <summary>
    /// Роль пользователя в мобильном приложении.
    /// </summary>
    public MobileUserRole Role { get; set; }

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
        int employeeId,
        string passwordHash,
        MobileUserRole role,
        DateTime? createdAtUtc = null)
    {
        if (employeeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(employeeId));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        EmployeeId = employeeId;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAt = createdAtUtc ?? DateTime.UtcNow;
        UpdatedAt = null;
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
