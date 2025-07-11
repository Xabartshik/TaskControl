using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InformationModule.Domain
{

    /// <summary>
    /// Сущность сотрудника компании
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Уникальный идентификатор сотрудника (первичный ключ)
        /// </summary>
        public int EmployeesId { get; set; }

        /// <summary>
        /// Фамилия сотрудника
        /// </summary>
        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        [StringLength(100, ErrorMessage = "Фамилия не может превышать 100 символов")]
        public string Surname { get; set; }

        /// <summary>
        /// Имя сотрудника
        /// </summary>
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [StringLength(100, ErrorMessage = "Имя не может превышать 100 символов")]
        public string Name { get; set; }

        /// <summary>
        /// Отчество сотрудника (необязательное поле)
        /// </summary>
        [StringLength(100, ErrorMessage = "Отчество не может превышать 100 символов")]
        public string? MiddleName { get; set; }

        /// <summary>
        /// Должность/роль сотрудника в компании
        /// </summary>
        [Required(ErrorMessage = "Должность обязательна для заполнения")]
        [StringLength(150, ErrorMessage = "Название должности не может превышать 150 символов")]
        public string Role { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата последнего обновления записи
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }


}
