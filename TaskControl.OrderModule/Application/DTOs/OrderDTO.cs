using System.Text.Json.Serialization;
using TaskControl.OrderModule.Domain;

namespace TaskControl.OrderModule.Application.DTOs
{
    public record OrderDto
    {
        public int OrderId { get; init; }

        public int CustomerId { get; init; }

        public int BranchId { get; init; }

        public DateTime? DeliveryDate { get; init; }

        // Поле для передачи выбранного окна
        public int? DeliverySlotId { get; init; }

        public string? DestinationAddress { get; set; }

        // Новые поля для связи с постаматами
        public int? PostamatId { get; init; }

        public int? PostamatCellId { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeliveryType DeliveryType { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentType PaymentType { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; init; } = OrderStatus.Created;

        public decimal TotalPrice { get; set; }

        public List<OrderPositionDto> Positions { get; set; } = new();

        public static Order FromDto(OrderDto dto) => new()
        {
            OrderId = dto.OrderId,
            CustomerId = dto.CustomerId,
            BranchId = dto.BranchId,
            DeliveryDate = dto.DeliveryDate,
            DeliverySlotId = dto.DeliverySlotId, // Маппинг
            DestinationAddress = dto.DestinationAddress,

            // Маппинг постаматов
            PostamatId = dto.PostamatId,
            PostamatCellId = dto.PostamatCellId,

            DeliveryType = dto.DeliveryType,
            PaymentType = dto.PaymentType,
            Status = dto.Status,
            TotalPrice = dto.TotalPrice,
        };

        public static OrderDto ToDto(Order entity) => new()
        {
            OrderId = entity.OrderId,
            CustomerId = entity.CustomerId,
            BranchId = entity.BranchId,
            DeliveryDate = entity.DeliveryDate,
            DeliverySlotId = entity.DeliverySlotId, // Маппинг
            DestinationAddress = entity.DestinationAddress,

            // Маппинг постаматов
            PostamatId = entity.PostamatId,
            PostamatCellId = entity.PostamatCellId,

            DeliveryType = entity.DeliveryType,
            PaymentType = entity.PaymentType,
            Status = entity.Status,
            TotalPrice = entity.TotalPrice // Восстановлен вывод суммы
        };
    }
}