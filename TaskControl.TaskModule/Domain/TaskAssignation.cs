using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//TODO: Пересмотреть сущность, добавить в нее некоторые поля, по типу имени пользователя, типа задачи и все такое
namespace TaskControl.TaskModule.Domain
{
    /// <summary>
    /// Назначение задачи на пользователя
    /// </summary>
    public class TaskAssignation
    {
        /// <summary>
        /// Уникальный идентификатор назначения
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор задачи
        /// </summary>
        [Required(ErrorMessage = "Не указана задача")]
        public int TaskId { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        [Required(ErrorMessage = "Не указан пользователь")]
        public int UserId { get; set; }

        /// <summary>
        /// Дата назначения
        /// </summary>
        [Required]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
