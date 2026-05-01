using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TaskControl.OrderModule.Domain
{
    public record DeliverySlot(int Id, string Name, TimeSpan StartTime, TimeSpan EndTime);

    public static class DeliverySchedule
    {
        public static readonly List<DeliverySlot> Slots = new()
        {
            new DeliverySlot(1, "Утро", new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0)),
            new DeliverySlot(2, "День", new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0)),
            new DeliverySlot(3, "Вечер", new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0))
        };
    }

    public enum OrderStatus
    {
        [Description("Создан")]
        Created,
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

    public enum DeliveryType
    {
        Pickup,
        Delivery,
        Postamat,
        Express
    }

    public enum PaymentType
    {
        Prepaid,
        Postpaid
    }

    /// <summary>
    /// Заказ в системе
    /// </summary>
    public class Order
    {
        [Required]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Не указан клиент")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Не указан филиал")]
        public int BranchId { get; set; }

        public DateTime? DeliveryDate { get; set; }

        // Новое поле: Идентификатор слота доставки
        public int? DeliverySlotId { get; set; }

        public string? DestinationAddress { get; set; }

        public int? PostamatId { get; set; }
        public int? PostamatCellId { get; set; }

        [Required]
        public DeliveryType DeliveryType { get; set; }

        [Required]
        public PaymentType PaymentType { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Created;

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}