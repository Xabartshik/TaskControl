using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.Domain;
using TaskControl.OrderModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Mapper
{
    public static class PositionMapper
    {
        public static PositionModel ToModel(this PositionCell entity)
        {
            if (entity == null) return null;

            return new PositionModel
            {
                PositionId = entity.PositionId,
                BranchId = entity.BranchId,
                Status = entity.Status,
                ZoneCode = entity.ZoneCode,
                FirstLevelStorageType = entity.FirstLevelStorageType,
                FLSNumber = entity.FLSNumber,
                SecondLevelStorage = entity.SecondLevelStorage,
                ThirdLevelStorage = entity.ThirdLevelStorage,
                Length = entity.Length,
                Width = entity.Width,
                Height = entity.Height,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static PositionCell ToDomain(this PositionModel model)
        {
            if (model == null) return null;

            return new PositionCell
            {
                PositionId = model.PositionId,
                BranchId = model.BranchId,
                Status = model.Status,
                ZoneCode = model.ZoneCode,
                FirstLevelStorageType = model.FirstLevelStorageType,
                FLSNumber = model.FLSNumber,
                SecondLevelStorage = model.SecondLevelStorage,
                ThirdLevelStorage = model.ThirdLevelStorage,
                Length = model.Length,
                Width = model.Width,
                Height = model.Height
            };
        }
    }
}
