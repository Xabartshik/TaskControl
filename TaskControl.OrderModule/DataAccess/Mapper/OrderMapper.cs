using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.OrderModule.Domain;

namespace TaskControl.OrderModule.DataAccess.Mapper
{
    public static class OrderMapper
    {
        // Order → OrderModel
        public static OrderModel ToModel(this Order entity)
        {
            if (entity == null) return null;

            return new OrderModel
            {
                OrderId = entity.OrderId,
                CustomerId = entity.CustomerId,
                BranchId = entity.BranchId,
                DeliveryDate = entity.DeliveryDate,
                Type = entity.Type,
                Status = entity.Status,
                // Установка времени создания
                CreatedAt = DateTime.UtcNow
            };
        }

        // OrderModel → Order
        public static Order ToDomain(this OrderModel model)
        {
            if (model == null) return null;

            return new Order
            {
                OrderId = model.OrderId,
                CustomerId = model.CustomerId,
                BranchId = model.BranchId,
                DeliveryDate = model.DeliveryDate,
                Type = model.Type,
                Status = model.Status
            };
        }
    }
}