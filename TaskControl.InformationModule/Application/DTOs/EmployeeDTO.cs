using System;
using System.ComponentModel.DataAnnotations;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.Application.DTOs
{
    public record EmployeeDto
    {
        /// <summary>
        /// Уникальный идентификатор сотрудника
        /// </summary>
        public int EmployeesId { get; init; }

        /// <summary>
        /// Фамилия сотрудника
        /// </summary>
        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        [StringLength(100, ErrorMessage = "Фамилия не может превышать 100 символов")]
        public string Surname { get; init; }

        /// <summary>
        /// Имя сотрудника
        /// </summary>
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [StringLength(100, ErrorMessage = "Имя не может превышать 100 символов")]
        public string Name { get; init; }

        /// <summary>
        /// Отчество сотрудника
        /// </summary>
        [StringLength(100, ErrorMessage = "Отчество не может превышать 100 символов")]
        public string? MiddleName { get; init; }

        /// <summary>
        /// Должность/роль сотрудника (Enum)
        /// </summary>
        [Required(ErrorMessage = "Должность обязательна для заполнения")]
        public WorkerRole Role { get; init; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// Преобразует сущность Employee в EmployeeDto
        /// </summary>
        public static EmployeeDto ToDto(Employee entity)
        {
            if (entity == null) return null;

            return new EmployeeDto
            {
                EmployeesId = entity.EmployeesId,
                Surname = entity.Surname,
                Name = entity.Name,
                MiddleName = entity.MiddleName,
                Role = entity.Role // Теперь передаем Enum напрямую
            };
        }

        /// <summary>
        /// Создает сущность Employee из EmployeeDto
        /// </summary>
        public static Employee FromDto(EmployeeDto dto)
        {
            if (dto == null) return null;

            return new Employee
            {
                EmployeesId = dto.EmployeesId,
                Surname = dto.Surname,
                Name = dto.Name,
                MiddleName = dto.MiddleName,
                Role = dto.Role // Принимаем Enum обратно
            };
        }
    }
}