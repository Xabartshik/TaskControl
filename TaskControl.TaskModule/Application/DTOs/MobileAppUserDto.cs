using System;
using System.ComponentModel.DataAnnotations;
using TaskControl.InformationModule.Domain;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.DTOs
{
    public record MobileAppUserDto
    {
        public int Id { get; init; }

        /// <summary>
        /// Универсальный логин (ID для сотрудников, Email/Телефон для клиентов)
        /// </summary>
        [Required]
        public string Login { get; init; } = null!;

        /// <summary>
        /// ID сотрудника (заполнено, если роль Worker/Supervisor/Admin)
        /// </summary>
        public int? EmployeeId { get; init; }

        /// <summary>
        /// ID покупателя (заполнено, если роль Customer)
        /// </summary>
        public int? CustomerId { get; init; }

        /// <summary>
        /// Роль пользователя в мобильном приложении (Enum)
        /// </summary>
        [Required]
        public MobileUserRole Role { get; init; }

        public WorkerRole? WorkerRole { get; set; }

        // Поля для отображения в UI (заполняются сервисом при получении данных)
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();

        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public int? BranchId { get; init; }

        /// <summary>
        /// Маппинг из DTO в доменную сущность
        /// </summary>
        public static MobileAppUser FromDto(MobileAppUserDto dto) => new()
        {
            Id = dto.Id,
            Login = dto.Login,
            EmployeeId = dto.EmployeeId,
            CustomerId = dto.CustomerId,
            Role = dto.Role,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            BranchId = dto.BranchId
        };

        /// <summary>
        /// Маппинг из доменной сущности в DTO
        /// </summary>
        public static MobileAppUserDto ToDto(MobileAppUser entity) => new()
        {
            Id = entity.Id,
            Login = entity.Login,
            EmployeeId = entity.EmployeeId,
            CustomerId = entity.CustomerId,
            Role = entity.Role,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            BranchId = entity.BranchId
        };
    }

    /// <summary>
    /// DTO для создания нового пользователя (как сотрудника, так и покупателя)
    /// </summary>
    public record CreateMobileAppUserDto
    {
        [Required]
        public string Login { get; init; } = null!;

        [Required]
        public string Password { get; init; } = null!;

        [Required]
        public MobileUserRole Role { get; init; }

        public int? EmployeeId { get; init; }
        public int? CustomerId { get; init; }
        public int? BranchId { get; init; }

        public static MobileAppUser FromDto(CreateMobileAppUserDto dto, string passwordHash) => new(
            login: dto.Login,
            passwordHash: passwordHash,
            role: dto.Role,
            employeeId: dto.EmployeeId,
            customerId: dto.CustomerId,
            branchId: dto.BranchId
        );
    }

    public record UpdateMobileUserRoleDto
    {
        [Required]
        public MobileUserRole Role { get; init; }

        public static void ApplyTo(MobileAppUser entity, UpdateMobileUserRoleDto dto)
        {
            entity.SetRole(dto.Role);
        }
    }

    public record UpdateMobileUserPasswordDto
    {
        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 символов")]
        public string Password { get; init; } = null!;
    }

    public record UpdateMobileUserActiveDto
    {
        [Required]
        public bool IsActive { get; init; }

        public static void ApplyTo(MobileAppUser entity, UpdateMobileUserActiveDto dto)
        {
            if (dto.IsActive)
                entity.Activate();
            else
                entity.Deactivate();
        }
    }
}