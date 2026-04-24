using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// Перечисление для стратегии распределения товаров
    /// </summary>
    public enum DivisionStrategy
    {
        /// <summary>
        /// Распределение по существующим зонам хранения
        /// </summary>
        ByZone = 0,

        /// <summary>
        /// Распределение по количеству товаров поровну
        /// </summary>
        ByQuantity = 1,

        /// <summary>
        /// Распределение по расстоянию (оптимизация маршрута)
        /// </summary>
        ByDistance = 2
    }

    /// <summary>
    /// DTO для создания и распределения инвентаризации между работниками
    /// </summary>
    public class CreateInventoryTaskDto
    {
        [Required(ErrorMessage = "Филиал обязателен")]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Список товаров обязателен")]
        [MinLength(1, ErrorMessage = "Минимум 1 товар")]
        public List<int> ItemPositionIds { get; set; } = new();

        [Range(0, 10, ErrorMessage = "Приоритет должен быть от 0 до 10")]
        public int Priority { get; set; } = 5;

        [Range(1, 50, ErrorMessage = "Количество работников от 1 до 50")]
        public int WorkerCount { get; set; } = 1;

        [StringLength(500, ErrorMessage = "Описание не может быть больше 500 символов")]
        public string? Description { get; set; }

        //TODO: Добавить разные стратегии для зоны хранения
        [Required(ErrorMessage = "Стратегия распределения обязательна")]
        public DivisionStrategy DivisionStrategy { get; set; } = DivisionStrategy.ByZone;

        public DateTime? DeadlineDate { get; set; }
    }
}
