using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Interface
{
    public class WorkerAssemblyTaskDto
    {
        public int AssignmentId { get; set; }
        public int TaskId { get; set; }
        public string? TaskNumber { get; set; }
        public int OrderId { get; set; }
        public OrderAssemblyAssignmentStatus Status { get; set; }
        public System.DateTime? CreatedDate { get; set; }
        public int TotalLines { get; set; }
        /// <summary>
        /// Список целевых ячеек с их товарами (для отображения кладовщику).
        /// </summary>
        public List<CellPlacementInfoDto> CellPlacements { get; set; } = new();
    }

    /// <summary>
    /// Информация о том, какие товары нужно разместить в конкретную ячейку PICKUP.
    /// </summary>
    public class CellPlacementInfoDto
    {
        public int TargetPositionId { get; set; }
        public string? CellCode { get; set; }
        public string? CellDisplayName { get; set; }
        public List<PlacementLineDto> Items { get; set; } = new();
    }

    public class PlacementLineDto
    {
        public int LineId { get; set; }
        public int ItemPositionId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? Barcode { get; set; }
        public int Quantity { get; set; }
        public OrderAssemblyLineStatus Status { get; set; }
    }

    /// <summary>
    /// Результат массового размещения товаров в ячейку PICKUP.
    /// </summary>
    public class BulkPlaceResultDto
    {
        public int PlacedCount { get; set; }
        public int RemainingCells { get; set; }
    }

    public interface IOrderAssemblyExecutionService
    {
        Task<List<WorkerAssemblyTaskDto>> GetWorkerAssemblyTasks(int userId);
        Task ScanAndPickItem(int lineId, string scannedBarcode);
        /// <summary>
        /// Массовое размещение: кладовщик сканирует ячейку PICKUP и все собранные (Picked) товары,
        /// предназначенные для этой ячейки, переводятся в статус Placed.
        /// </summary>
        Task<BulkPlaceResultDto> ScanAndPlaceBulk(int assignmentId, string scannedCellCode);
        Task ReportMissingItem(int lineId, string reason);
        Task CompleteAssemblyTask(int assignmentId);
    }
}
