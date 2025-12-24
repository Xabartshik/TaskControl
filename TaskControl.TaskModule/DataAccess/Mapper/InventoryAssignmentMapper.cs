using TaskControl.InventoryModule.Domain;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Mapper;

public static class InventoryAssignmentMapper
{
    public static InventoryAssignment ToDomain(this InventoryAssignmentModel model)
    {
        if (model is null)
            return null;

        return new InventoryAssignment(
            id: model.Id,
            taskId: model.TaskId,
            assignedToUserId: model.AssignedToUserId,
            branchId: model.BranchId,
            assignedAtUtc: model.AssignedAt,
            lines: new List<InventoryAssignmentLine>())
        {
            CompletedAt = model.CompletedAt,
            Status = (InventoryAssignmentStatus)model.Status
        };
    }

    public static InventoryAssignmentModel ToModel(this InventoryAssignment domain)
    {
        if (domain is null)
            return null;

        return new InventoryAssignmentModel
        {
            Id = domain.Id,
            TaskId = domain.TaskId,
            AssignedToUserId = domain.AssignedToUserId,
            BranchId = domain.BranchId,
            Status = (int)domain.Status,
            AssignedAt = domain.AssignedAt,
            CompletedAt = domain.CompletedAt
        };
    }
}

public static class InventoryAssignmentLineMapper
{
    public static InventoryAssignmentLine ToDomain(this InventoryAssignmentLineModel model)
    {
        if (model is null)
            return null;

        var positionCode = new PositionCode
        {
            BranchId = 0,
            ZoneCode = model.ZoneCode,
            FirstLevelStorageType = model.FirstLevelStorageType,
            FLSNumber = model.FLSNumber,
            SecondLevelStorage = model.SecondLevelStorage,
            ThirdLevelStorage = model.ThirdLevelStorage
        };

        return new InventoryAssignmentLine(
            id: model.Id,
            inventoryAssignmentId: model.InventoryAssignmentId,
            itemPositionId: model.ItemPositionId,
            positionId: model.PositionId,
            positionCode: positionCode,
            expectedQuantity: model.ExpectedQuantity)
        {
            ActualQuantity = model.ActualQuantity
        };
    }

    public static InventoryAssignmentLineModel ToModel(this InventoryAssignmentLine domain)
    {
        if (domain is null)
            return null;

        return new InventoryAssignmentLineModel
        {
            Id = domain.Id,
            InventoryAssignmentId = domain.InventoryAssignmentId,
            ItemPositionId = domain.ItemPositionId,
            PositionId = domain.PositionId,
            ExpectedQuantity = domain.ExpectedQuantity,
            ActualQuantity = domain.ActualQuantity,
            ZoneCode = domain.PositionCode.ZoneCode,
            FirstLevelStorageType = domain.PositionCode.FirstLevelStorageType,
            FLSNumber = domain.PositionCode.FLSNumber,
            SecondLevelStorage = domain.PositionCode.SecondLevelStorage,
            ThirdLevelStorage = domain.PositionCode.ThirdLevelStorage
        };
    }
}
