using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// Должность/роль сотрудника
        /// </summary>
        [Required(ErrorMessage = "Должность обязательна для заполнения")]
        [StringLength(150, ErrorMessage = "Название должности не может превышать 150 символов")]
        public string Role { get; init; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// Преобразует сущность Employee в EmployeeDto
        /// </summary>
        public static EmployeeDto ToDto(Employee entity)
        {
            return new EmployeeDto
            {
                EmployeesId = entity.EmployeesId,
                Surname = entity.Surname,
                Name = entity.Name,
                MiddleName = entity.MiddleName,
                Role = entity.Role
            };
        }

        /// <summary>
        /// Создает сущность Employee из EmployeeDto
        /// </summary>
        public static Employee FromDto(EmployeeDto dto)
        {
            return new Employee
            {
                EmployeesId = dto.EmployeesId,
                Surname = dto.Surname,
                Name = dto.Name,
                MiddleName = dto.MiddleName,
                Role = dto.Role
            };
        }
    }

}