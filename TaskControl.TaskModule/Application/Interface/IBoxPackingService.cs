using System.Collections.Generic;

namespace TaskControl.TaskModule.Application.Services
{
    public class ItemToPack
    {
        public int OrderPositionId { get; set; }
        public int ItemId { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class CellToPackInto
    {
        public int PositionId { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class PackingResult
    {
        /// <summary>
        /// Key: OrderPositionId, Value: Target PositionId (ячейка PICKUP)
        /// </summary>
        public Dictionary<int, int> ItemToCellMap { get; set; } = new();
    }

    public interface IBoxPackingService
    {
        PackingResult AssignItemsToPickupCells(List<ItemToPack> items, List<CellToPackInto> cells);
    }
}
