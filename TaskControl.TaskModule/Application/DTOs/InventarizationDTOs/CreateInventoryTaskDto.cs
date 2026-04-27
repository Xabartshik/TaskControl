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
        public int BranchId { get; set; }
        public List<int> ItemPositionIds { get; set; } = new();

        /// <summary>
        /// Если список пуст, система выберет работников автоматически.
        /// </summary>
        public List<int>? WorkerIds { get; set; }

        public int WorkerCount { get; set; }
        public DivisionStrategy DivisionStrategy { get; set; }
        public int PriorityLevel { get; set; }
        public DateTime? DeadlineDate { get; set; }
    }
}
