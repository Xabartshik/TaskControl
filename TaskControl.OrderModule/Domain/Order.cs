using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.OrderModule.Domain
{
    using global::TaskControl.OrderModule.Domain.TaskControl.OrderModule.Domain.Enums;
    using System.ComponentModel;

    public enum OrderStatus
    {
        [Description("Создан")]
        Created,

        [Description("Резерв подтвержден: товары есть в наличии")]
        Reserved,

        [Description("В процессе сборки")]
        Assembly,

        [Description("Собран и ожидает выдачи/доставки")]
        Ready,

        [Description("В пути — для курьера или постамата")]
        InTransit,

        [Description("Завершен")]
        Completed,

        [Description("Отменен")]
        Canceled
    }

    namespace TaskControl.OrderModule.Domain.Enums
    {
        public enum DeliveryType
        {
            Pickup,     // Обычный самовывоз
            Delivery,   // Доставка курьером
            Postamat,   // Постамат
            Express     // Сборка и выдача здесь и сейчас
        }
    }

    namespace TaskControl.OrderModule.Domain.Enums
    {
        public enum PaymentType
        {
            Prepaid,    // С предоплатой
            Postpaid    // Без предоплаты (оплата при получении)
        }
    }
    /// <summary>
    /// Заказ в системе
    /// </summary>
    public class Order
    {
        [Required]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Не указан клиент")]
        public int CustomerId { get; set; } // Имя клиента получаем из другого микросервиса по ID

        [Required(ErrorMessage = "Не указан филиал")]
        public int BranchId { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public string? DestinationAddress { get; set; }

        [Required]
        public DeliveryType DeliveryType { get; set; }

        [Required]
        public PaymentType PaymentType { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Created;

        public DateTime CreatedAt { get; set; }
    }
}