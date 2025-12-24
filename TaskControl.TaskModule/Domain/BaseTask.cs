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
    public enum TaskStatus
    {
        New = 0,
        Assigned = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        OnHold = 5,
        Blocked = 6
    }

    /// <summary>
    /// Активная задача в системе
    /// </summary>
    public class BaseTask
    {
        /// <summary>
        /// Уникальный идентификатор задачи
        /// </summary>
        [Required]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "Название задачи обязательно")]
        [StringLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        public string Title { get; set; }


        [StringLength(2000, ErrorMessage = "Описание не может превышать 2000 символов")]
        public string? Description { get; set; }
        /// <summary>
        /// Идентификатор филиала
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// Тип задачи (Нужен для оптимизации работы фабрики)
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
        public TaskStatus Status { get; set; } = TaskStatus.New;


        [Range(0, 10)]
        public int Priority { get; set; } = 5;
        /// <summary>
        /// Дополнительные параметры задачи в формате JSON были удалены, так как являются частью БД, а не бизнес-сущности
        /// </summary>

        /// <summary>
        /// Проверяет, является ли задача активной
        /// </summary>
        public bool IsActive() => Status != TaskStatus.Completed
            && Status != TaskStatus.Cancelled;
    }
}
