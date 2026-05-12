using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InformationModule.Domain
{
    public enum WorkerRole
    {
        Storekeeper = 1,  // Сборщик заказов
        OrderIssuer = 2,  // Кассир (Выдает заказы)
        Manager = 3,      // Начальник
        Courier = 4       // Курьер
    }
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
        /// Должность/роль сотрудника в компании TODO: заменить на тип, убрав строку
        /// </summary>
        public WorkerRole Role { get; set; }

    }


}
