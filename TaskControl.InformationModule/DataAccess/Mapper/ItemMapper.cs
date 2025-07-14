using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Mapper
{
    public static class ItemMapper
    {
        public static ItemModel ToModel(this Item entity)
        {
            if (entity == null) return null;

            return new ItemModel
            {
                ItemId = entity.ItemId,
                Weight = entity.Weight,
                Length = entity.Length,
                Width = entity.Width,
                Height = entity.Height,
                CreatedAt = DateTime.UtcNow // Устанавливается при создании
            };
        }

        public static Item ToDomain(this ItemModel model)
        {
            if (model == null) return null;

            return new Item
            {
                ItemId = model.ItemId,
                Weight = model.Weight,
                Length = model.Length,
                Width = model.Width,
                Height = model.Height
                // CreatedAt не передается в бизнес-сущность
            };
        }
    }
}
