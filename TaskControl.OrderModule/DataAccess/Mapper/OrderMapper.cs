using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.OrderModule.Domain;

namespace TaskControl.OrderModule.DataAccess.Mapper
{
    public static class OrderMapper
    {
        public static OrderModel ToModel(this Order entity)
        {
            if (entity == null) return null;

            return new OrderModel
            {
                OrderId = entity.OrderId,
                CustomerId = entity.CustomerId,
                BranchId = entity.BranchId,
                DeliveryDate = entity.DeliveryDate,
                DeliverySlotId = entity.DeliverySlotId, // Маппинг слота
                DestinationAddress = entity.DestinationAddress,

                // Трансформация для БД
                DeliveryType = entity.DeliveryType.ToString(),
                PaymentType = entity.PaymentType.ToString(),
                PostamatId = entity.PostamatId,
                PostamatCellId = entity.PostamatCellId,
                Status = entity.Status.ToString(),

                TotalPrice = entity.TotalPrice,

                CreatedAt = entity.CreatedAt == default ? DateTime.UtcNow : entity.CreatedAt
            };
        }

        public static Order ToDomain(this OrderModel model)
        {
            if (model == null) return null;

            return new Order
            {
                OrderId = model.OrderId,
                CustomerId = model.CustomerId,
                BranchId = model.BranchId,
                DeliveryDate = model.DeliveryDate,
                DeliverySlotId = model.DeliverySlotId, // Маппинг слота
                DestinationAddress = model.DestinationAddress,
                PostamatId = model.PostamatId,
                PostamatCellId = model.PostamatCellId,
                // Безопасный парсинг из БД
                DeliveryType = Enum.TryParse<DeliveryType>(model.DeliveryType, out var dType) ? dType : DeliveryType.Pickup,
                PaymentType = Enum.TryParse<PaymentType>(model.PaymentType, out var pType) ? pType : PaymentType.Postpaid,
                Status = Enum.TryParse<OrderStatus>(model.Status, out var status) ? status : OrderStatus.Created,
                TotalPrice = model.TotalPrice,
                CreatedAt = model.CreatedAt
            };
        }
    }
}