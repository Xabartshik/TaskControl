using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Mapper
{
    public static class ItemPositionMapper
    {
        public static ItemPositionModel ToModel(this ItemPosition entity)
        {
            if (entity == null) return null;

            return new ItemPositionModel
            {
                Id = entity.Id,
                ItemId = entity.ItemId,
                PositionId = entity.PositionId,
                Quantity = entity.Quantity,
                Length = entity.Length,
                Width = entity.Width,
                Height = entity.Height,
            };
        }

        public static ItemPosition ToDomain(this ItemPositionModel model)
        {
            if (model == null) return null;

            return new ItemPosition
            {
                Id = model.Id,
                ItemId = model.ItemId,
                PositionId = model.PositionId,
                Quantity = model.Quantity,
                Length = model.Length,
                Width = model.Width,
                Height = model.Height,
            };
        }
    }
}
