using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

//TODO: Подумать, нужно ли хранить задачи уже выполненные?

namespace TaskControl.TaskModule.Domain
{
    /// <summary>
    /// Активная задача в системе
    /// </summary>
    public class ActiveTask
    {
        /// <summary>
        /// Уникальный идентификатор задачи
        /// </summary>
        [Required]
        public int TaskId { get; set; }

        /// <summary>
        /// Идентификатор филиала
        /// </summary>
        [Required(ErrorMessage = "Не указан филиал")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный идентификатор филиала")]
        public int BranchId { get; set; }

        /// <summary>
        /// Тип задачи
        /// </summary>
        [Required(ErrorMessage = "Тип задачи обязателен")]
        [StringLength(50, ErrorMessage = "Тип задачи не может превышать 50 символов")]
        public string Type { get; set; }

        /// <summary>
        /// Дата создания задачи
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата завершения задачи (null если задача активна)
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Статус задачи
        /// </summary>
        [Required]
        [RegularExpression("^(New|InProgress|Completed|Cancelled)$",
            ErrorMessage = "Недопустимый статус задачи")]
        public string Status { get; set; } = "New";

        /// <summary>
        /// Дополнительные параметры задачи в формате JSON
        /// </summary>
        public JsonDocument? JSONParams { get; set; }

        /// <summary>
        /// Проверяет, является ли задача активной
        /// </summary>
        public bool IsActive() => Status != "Completed" && Status != "Cancelled";
    }
}
