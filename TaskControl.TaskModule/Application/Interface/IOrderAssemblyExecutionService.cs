using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Interface
{
    public class OrderAssemblyHeaderDto
    {
        public int AssignmentId { get; set; }
        public int TaskId { get; set; }
        public int OrderId { get; set; }
        public AssignmentStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public int TotalLines { get; set; }
        public int PlacedLines { get; set; }
        public double CompletionPercentage => TotalLines == 0 ? 0 : Math.Round((double)PlacedLines / TotalLines * 100, 1);
    }

    public class WorkerAssemblyTaskDto
    {
        public int AssignmentId { get; set; }
        public int TaskId { get; set; }
        public string? TaskNumber { get; set; }
        public int OrderId { get; set; }
        public AssignmentStatus Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? Deadline { get; set; }
        public int TotalLines { get; set; }
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

        public string SourceCellCode { get; set; }
        public int PickedQuantity { get; set; }
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
        Task<List<OrderAssemblyHeaderDto>> GetAssignmentsHeaderForWorkerAsync(int userId);
        Task<WorkerAssemblyTaskDto> GetAssemblyTaskDetailsAsync(int assignmentId);

        Task<bool> StartAssemblyAsync(int assignmentId);
        Task<bool> PauseAssemblyAsync(int assignmentId);
        Task<bool> CancelAssemblyAsync(int assignmentId);

        Task ScanAndPickItem(int lineId, string scannedBarcode);
        Task<BulkPlaceResultDto> ScanAndPlaceBulk(int assignmentId, string scannedCellCode);
        Task ReportMissingItem(int lineId, string reason);
        Task CompleteAssemblyTask(int assignmentId);
    }
}
