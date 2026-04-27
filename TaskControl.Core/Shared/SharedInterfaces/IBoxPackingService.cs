using System.Collections.Generic;

namespace TaskControl.Core.Shared.SharedInterfaces
{
    public class ItemToPack
    {
        public int OrderPositionId { get; set; }
        public int ItemId { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int Quantity { get; set; }
    }

    public class CellToPackInto
    {
        public int PositionId { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class PackedItemResult
    {
        public int OrderPositionId { get; set; }
        public int TargetPositionId { get; set; }
        public int Quantity { get; set; }
    }

    public class PackingResult
    {
        public List<PackedItemResult> PackedItems { get; set; } = new();
        public bool IsFullyPacked { get; set; }
    }

    public interface IBoxPackingService
    {
        PackingResult AssignItemsToPickupCells(List<ItemToPack> items, List<CellToPackInto> cells);
    }
}