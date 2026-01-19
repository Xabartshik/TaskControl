using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.DTOs
{
    public record MobileAppUserDto
    {
        public int Id { get; init; }

        [Required]
        public int EmployeeId { get; init; }

        [Required]
        [StringLength(30)]
        public string Role { get; init; } = null!;

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public string FullName => $"{FirstName} {LastName}";

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? UpdatedAt { get; init; }

        public static MobileAppUser FromDto(MobileAppUserDto dto) => new()
        {
            Id = dto.Id,
            EmployeeId = dto.EmployeeId,
            Role = Enum.TryParse<MobileUserRole>(dto.Role, ignoreCase: true, out var parsed)
                   ? parsed
                   : throw new ArgumentException($"Unknown mobile user role: {dto.Role}", nameof(dto.Role)),
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };

        public static MobileAppUserDto ToDto(MobileAppUser entity) => new()
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            Role = entity.Role.ToString(),
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public record CreateMobileAppUserDto
    {
        [Required]
        public int EmployeeId { get; init; }

        [Required]
        public string Password { get; init; } = null!;

        [Required]
        [StringLength(30)]
        public string Role { get; init; } = "Worker";

        public static MobileAppUser FromDto(CreateMobileAppUserDto dto, string passwordHash) => new()
        {
            EmployeeId = dto.EmployeeId,
            Role = Enum.TryParse<MobileUserRole>(dto.Role, ignoreCase: true, out var parsed)
                   ? parsed
                   : throw new ArgumentException($"Unknown mobile user role: {dto.Role}", nameof(dto.Role))
        };
    }

    public record UpdateMobileUserPasswordDto
    {
        [Required]
        public string Password { get; init; } = null!;

        //public static void ApplyTo(MobileAppUser entity, UpdateMobileUserPasswordDto dto, string passwordHash)
        //{
        //    // Логика обновления пароля будет в доменном объекте или сервисе
        //}
    }

    public record UpdateMobileUserRoleDto
    {
        [Required]
        [StringLength(30)]
        public string Role { get; init; } = null!;

        public static void ApplyTo(MobileAppUser entity, UpdateMobileUserRoleDto dto)
        {
            var roleEnum = Enum.TryParse<MobileUserRole>(dto.Role, ignoreCase: true, out var parsed)
                           ? parsed
                           : throw new ArgumentException($"Unknown mobile user role: {dto.Role}", nameof(dto.Role));

            entity.SetRole(roleEnum);
        }
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