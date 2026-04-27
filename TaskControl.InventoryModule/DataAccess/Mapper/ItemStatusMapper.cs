using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Mapper
{
    public static class ItemStatusMapper
    {
        public static ItemStatusModel ToModel(this ItemStatus entity)
        {
            if (entity == null) return null;

            return new ItemStatusModel
            {
                Id = entity.Id,
                ItemPositionId = entity.ItemPositionId,
                Status = entity.Status,
                StatusDate = entity.StatusDate,
                Quantity = entity.Quantity
            };
        }

        public static ItemStatus ToDomain(this ItemStatusModel model)
        {
            if (model == null) return null;

            return new ItemStatus
            {
                Id = model.Id,
                ItemPositionId = model.ItemPositionId,
                Status = model.Status,
                StatusDate = model.StatusDate,
                Quantity = model.Quantity
            };
        }
    }
}
