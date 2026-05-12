using System;
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
                ItemId = entity.ItemId,
                SourcePositionId = entity.SourcePositionId,
                DestinationPositionId = entity.DestinationPositionId,
                SourceBranchId = entity.SourceBranchId,
                DestinationBranchId = entity.DestinationBranchId,
                Quantity = entity.Quantity,
                WorkerId = entity.WorkerId,
                TaskId = entity.TaskId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static ItemMovement ToDomain(this ItemMovementModel model)
        {
            if (model == null) return null;

            return new ItemMovement
            {
                Id = model.Id,
                ItemId = model.ItemId,
                SourcePositionId = model.SourcePositionId,
                DestinationPositionId = model.DestinationPositionId,
                SourceBranchId = model.SourceBranchId,
                DestinationBranchId = model.DestinationBranchId,
                Quantity = model.Quantity,
                WorkerId = model.WorkerId,
                TaskId = model.TaskId
            };
        }
    }
}