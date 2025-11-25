using System;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.Domain;
using UnitsNet;

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
                BranchId = entity.Code.BranchId,
                Status = entity.Status,
                ZoneCode = entity.Code.ZoneCode,
                FirstLevelStorageType = entity.Code.FirstLevelStorageType,
                FLSNumber = entity.Code.FLSNumber,
                SecondLevelStorage = entity.Code.SecondLevelStorage,
                ThirdLevelStorage = entity.Code.ThirdLevelStorage,
                Length = entity.Length.Millimeters,
                Width = entity.Width.Millimeters,
                Height = entity.Height.Millimeters,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static PositionCell ToDomain(this PositionModel model)
        {
            if (model == null) return null;

            return new PositionCell
            {
                PositionId = model.PositionId,
                Code = new PositionCode
                {
                    BranchId = model.BranchId,
                    ZoneCode = model.ZoneCode,
                    FirstLevelStorageType = model.FirstLevelStorageType,
                    FLSNumber = model.FLSNumber,
                    SecondLevelStorage = model.SecondLevelStorage,
                    ThirdLevelStorage = model.ThirdLevelStorage
                },
                Status = model.Status,
                Length = Length.FromMillimeters(model.Length),
                Width = Length.FromMillimeters(model.Width),
                Height = Length.FromMillimeters(model.Height)
            };
        }
    }
}
