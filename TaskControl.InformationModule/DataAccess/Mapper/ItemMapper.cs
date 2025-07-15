using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InformationModule.Domain;
using UnitsNet;

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
                Weight = entity.Weight.Kilograms,
                Length = entity.Length.Millimeters,
                Width = entity.Width.Millimeters,
                Height = entity.Height.Millimeters,
                CreatedAt = DateTime.UtcNow // Устанавливается при создании
            };
        }

        public static Item ToDomain(this ItemModel model)
        {
            if (model == null) return null;

            return new Item
            {
                ItemId = model.ItemId,
                Weight = Mass.FromGrams(model.Weight),
                Length = Length.FromMillimeters(model.Length),
                Width = Length.FromMillimeters(model.Width),
                Height = Length.FromMillimeters(model.Width)
            };
        }
    }
}
