using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//TODO: Доработать сущность OrderPosition
namespace TaskControl.InventoryModule.Domain
{
    /// <summary>
    /// Связь заказа с позицией на складе
    /// </summary>
    public class OrderPosition
    {
        [Required]
        public int UniqueId { get; set; }

        [Required(ErrorMessage = "Не указан заказ")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID заказа")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Не указана позиция")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID позиции")]
        public int ItemPositionId { get; set; }

        [Required(ErrorMessage = "Укажите количество")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Количество должно быть положительным")]
        public int Quantity { get; set; }

    }
}
