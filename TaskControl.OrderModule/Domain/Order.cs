using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.OrderModule.Domain
{
    /// <summary>
    /// Заказ в системе
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Уникальный идентификатор заказа
        /// </summary>
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        [Required(ErrorMessage = "Не указан клиент")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID клиента")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Идентификатор филиала
        /// </summary>
        [Required(ErrorMessage = "Не указан филиал")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID филиала")]
        public int BranchId { get; set; }

        /// <summary>
        /// Дата доставки (обязательна для доставляемых заказов)
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        //TODO: Пересмотреть типы заказа (Доставка, Перегон и т.д.)

        /// <summary>
        /// Тип заказа (Online/Offline/Wholesale)
        /// </summary>
        [Required(ErrorMessage = "Тип заказа обязателен")]
        [RegularExpression("^(Online|Offline)$",
            ErrorMessage = "Допустимые типы: Online, Offline")]
        public string Type { get; set; }

        /// <summary>
        /// Статус заказа
        /// </summary>
        [Required]
        [RegularExpression("^(New|Processing|Delivered|Cancelled)$",
            ErrorMessage = "Недопустимый статус заказа")]
        public string Status { get; set; } = "New";

        /// <summary>
        /// Проверяет, требует ли заказ доставки
        /// </summary>
        public bool RequiresDelivery() => Type == "Online";

    }
}