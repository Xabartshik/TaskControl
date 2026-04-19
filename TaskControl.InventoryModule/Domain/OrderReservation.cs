using System;
using System.ComponentModel.DataAnnotations;

namespace TaskControl.InventoryModule.Domain
{
    /// <summary>
    /// Резервирование конкретной позиции на складе под заказ
    /// </summary>
    public class OrderReservation
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Не указана позиция заказа")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID позиции заказа")]
        public int OrderPositionId { get; set; }

        public int? ItemPositionId { get; set; }

        [Required(ErrorMessage = "Укажите количество")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Количество должно быть положительным")]
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
