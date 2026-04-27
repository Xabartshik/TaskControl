using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// DTO для обработки сканирования товара при инвентаризации
    /// </summary>
    public class ProcessInventoryScanDto
    {
        [Required(ErrorMessage = "ID назначения обязателен")]
        public int AssignmentId { get; set; }

        [Required(ErrorMessage = "ID строки обязателен")]
        public int LineId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int ActualQuantity { get; set; }

        [Required(ErrorMessage = "ID пользователя обязателен")]
        public int UserId { get; set; }

        [StringLength(500, ErrorMessage = "Заметка не может быть больше 500 символов")]
        public string? Note { get; set; }
    }

}
