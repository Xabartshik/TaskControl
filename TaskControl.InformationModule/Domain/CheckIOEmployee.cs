using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InformationModule.Domain
{
    /// <summary>
    /// Запись о приходе/уходе сотрудника в филиале
    /// </summary>
    public class CheckIOEmployee
    {
        /// <summary>
        /// Уникальный идентификатор записи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор сотрудника (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Не указан сотрудник")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный идентификатор сотрудника")]
        public int EmployeeId { get; set; }

        /// <summary>
        /// Идентификатор филиала (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Не указан филиал")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный идентификатор филиала")]
        public int BranchId { get; set; }

        /// <summary>
        /// Тип отметки (вход/выход) (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Не указан тип отметки")]
        [RegularExpression("^(in|out)$", ErrorMessage = "Допустимые значения: 'in' (вход) или 'out' (выход)")]
        public string CheckType { get; set; }

        /// <summary>
        /// Время отметки (устанавливается автоматически при создании)
        /// </summary>
        public DateTime CheckTimeStamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Проверяет, является ли отметка входом
        /// </summary>
        public bool IsCheckIn() => CheckType?.ToLower() == "in";

        /// <summary>
        /// Проверяет, является ли отметка выходом
        /// </summary>
        public bool IsCheckOut() => CheckType?.ToLower() == "out";
    }
}
