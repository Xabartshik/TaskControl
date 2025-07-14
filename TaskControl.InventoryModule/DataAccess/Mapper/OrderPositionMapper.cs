using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Mapper
{
    public static class OrderPositionMapper
    {
        public static OrderPositionModel ToModel(this OrderPosition entity)
        {
            if (entity == null) return null;

            return new OrderPositionModel
            {
                UniqueId = entity.UniqueId,
                OrderId = entity.OrderId,
                ItemPositionId = entity.ItemPositionId,
                Quantity = entity.Quantity
            };
        }

        public static OrderPosition ToDomain(this OrderPositionModel model)
        {
            if (model == null) return null;

            return new OrderPosition
            {
                UniqueId = model.UniqueId,
                OrderId = model.OrderId,
                ItemPositionId = model.ItemPositionId,
                Quantity = model.Quantity
            };
        }
    }
}
