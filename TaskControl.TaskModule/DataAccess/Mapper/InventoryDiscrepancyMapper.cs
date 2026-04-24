using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Mapper;

public static class InventoryDiscrepancyMapper
{
    public static InventoryDiscrepancy ToDomain(this InventoryDiscrepancyModel model)
    {
        if (model is null)
            return null;

        return new InventoryDiscrepancy
        {
            Id = model.Id,
            InventoryAssignmentLineId = model.InventoryAssignmentLineId,
            ItemPositionId = model.ItemPositionId,
            ExpectedQuantity = model.ExpectedQuantity,
            ActualQuantity = model.ActualQuantity,
            Type = (DiscrepancyType)model.Type,
            Note = model.Note,
            IdentifiedAt = model.IdentifiedAt,
            ResolutionStatus = (DiscrepancyResolutionStatus)model.ResolutionStatus
        };
    }

    public static InventoryDiscrepancyModel ToModel(this InventoryDiscrepancy domain)
    {
        if (domain is null)
            return null;

        return new InventoryDiscrepancyModel
        {
            Id = domain.Id,
            InventoryAssignmentLineId = domain.InventoryAssignmentLineId,
            ItemPositionId = domain.ItemPositionId,
            ExpectedQuantity = domain.ExpectedQuantity,
            ActualQuantity = domain.ActualQuantity,
            Type = (int)domain.Type,
            Note = domain.Note,
            IdentifiedAt = domain.IdentifiedAt,
            ResolutionStatus = (int)domain.ResolutionStatus
        };
    }
}
