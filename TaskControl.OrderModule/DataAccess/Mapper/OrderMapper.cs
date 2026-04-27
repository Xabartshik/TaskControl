using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.OrderModule.Domain;
using TaskControl.OrderModule.Domain.TaskControl.OrderModule.Domain.Enums;

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
                DestinationAddress = entity.DestinationAddress,

                // Трансформация для БД
                DeliveryType = entity.DeliveryType.ToString(),
                PaymentType = entity.PaymentType.ToString(),
                Status = entity.Status.ToString(),

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
                DestinationAddress = model.DestinationAddress,

                // Безопасный парсинг из БД
                DeliveryType = Enum.TryParse<DeliveryType>(model.DeliveryType, out var dType) ? dType : DeliveryType.Pickup,
                PaymentType = Enum.TryParse<PaymentType>(model.PaymentType, out var pType) ? pType : PaymentType.Postpaid,
                Status = Enum.TryParse<OrderStatus>(model.Status, out var status) ? status : OrderStatus.Created,

                CreatedAt = model.CreatedAt
            };
        }
    }
}