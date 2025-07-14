using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Mapper
{
    public static class ItemMovementMapper
    {
        public static ItemMovementModel ToModel(this ItemMovement entity)
        {
            if (entity == null) return null;

            return new ItemMovementModel
            {
                Id = entity.Id,
                SourceItemPositionId = entity.SourceItemPositionId,
                DestinationPositionId = entity.DestinationPositionId,
                SourceBranchId = entity.SourceBranchId,
                DestinationBranchId = entity.DestinationBranchId,
                Quantity = entity.Quantity
            };
        }

        public static ItemMovement ToDomain(this ItemMovementModel model)
        {
            if (model == null) return null;

            return new ItemMovement
            {
                Id = model.Id,
                SourceItemPositionId = model.SourceItemPositionId,
                DestinationPositionId = model.DestinationPositionId,
                SourceBranchId = model.SourceBranchId,
                DestinationBranchId = model.DestinationBranchId,
                Quantity = model.Quantity
            };
        }
    }
}
