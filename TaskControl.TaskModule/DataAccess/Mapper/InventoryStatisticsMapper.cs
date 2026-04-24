using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Mapper;

public static class InventoryStatisticsMapper
{
    public static InventoryStatistics ToDomain(this InventoryStatisticsModel model)
    {
        if (model is null)
            return null;

        return new InventoryStatistics
        {
            Id = model.Id,
            InventoryAssignmentId = model.InventoryAssignmentId,
            TotalPositions = model.TotalPositions,
            CountedPositions = model.CountedPositions,
            DiscrepancyCount = model.DiscrepancyCount,
            SurplusCount = model.SurplusCount,
            ShortageCount = model.ShortageCount,
            TotalSurplusQuantity = model.TotalSurplusQuantity,
            TotalShortageQuantity = model.TotalShortageQuantity,
            StartedAt = model.StartedAt,
            CompletedAt = model.CompletedAt
        };
    }

    public static InventoryStatisticsModel ToModel(this InventoryStatistics domain)
    {
        if (domain is null)
            return null;

        return new InventoryStatisticsModel
        {
            Id = domain.Id,
            InventoryAssignmentId = domain.InventoryAssignmentId,
            TotalPositions = domain.TotalPositions,
            CountedPositions = domain.CountedPositions,
            DiscrepancyCount = domain.DiscrepancyCount,
            SurplusCount = domain.SurplusCount,
            ShortageCount = domain.ShortageCount,
            TotalSurplusQuantity = domain.TotalSurplusQuantity,
            TotalShortageQuantity = domain.TotalShortageQuantity,
            StartedAt = domain.StartedAt,
            CompletedAt = domain.CompletedAt
        };
    }
}
