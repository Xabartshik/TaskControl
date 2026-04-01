using System;

namespace TaskControl.TaskModule.Domain
{
    public enum OrderAssemblyLineStatus
    {
        Pending = 0,
        Picked = 1,
        Placed = 2,
        Discrepancy = 3
    }

    /// <summary>
    /// Строка назначения сборки: что взять, откуда и куда положить.
    /// </summary>
    public class OrderAssemblyLine
    {
        public int Id { get; internal set; }

        /// <summary>
        /// Ссылка на назначение (родительский агрегат).
        /// </summary>
        public int OrderAssemblyAssignmentId { get; internal set; }

        /// <summary>
        /// Id ItemPosition (товар).
        /// </summary>
        public int ItemPositionId { get; internal set; }

        /// <summary>
        /// Id складской ячейки (PositionCell.PositionId), откуда забираем товар.
        /// </summary>
        public int SourcePositionId { get; internal set; }

        /// <summary>
        /// Id складской ячейки (PositionCell.PositionId), куда кладем товар (PICKUP).
        /// </summary>
        public int TargetPositionId { get; internal set; }

        /// <summary>
        /// Количество товара для сборки.
        /// </summary>
        public int Quantity { get; internal set; }

        public OrderAssemblyLineStatus Status { get; internal set; }

        internal OrderAssemblyLine() { }

        public OrderAssemblyLine(
            int id,
            int orderAssemblyAssignmentId,
            int itemPositionId,
            int sourcePositionId,
            int targetPositionId,
            int quantity,
            OrderAssemblyLineStatus status)
        {
            if (orderAssemblyAssignmentId <= 0)
                throw new ArgumentOutOfRangeException(nameof(orderAssemblyAssignmentId));
            if (itemPositionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(itemPositionId));
            if (sourcePositionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(sourcePositionId));
            if (targetPositionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(targetPositionId));
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity));

            Id = id;
            OrderAssemblyAssignmentId = orderAssemblyAssignmentId;
            ItemPositionId = itemPositionId;
            SourcePositionId = sourcePositionId;
            TargetPositionId = targetPositionId;
            Quantity = quantity;
            Status = status;
        }

        public OrderAssemblyLine(
            int orderAssemblyAssignmentId,
            int itemPositionId,
            int sourcePositionId,
            int targetPositionId,
            int quantity)
        {
            if (orderAssemblyAssignmentId <= 0)
                throw new ArgumentOutOfRangeException(nameof(orderAssemblyAssignmentId));
            if (itemPositionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(itemPositionId));
            if (sourcePositionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(sourcePositionId));
            if (targetPositionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(targetPositionId));
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity));

            OrderAssemblyAssignmentId = orderAssemblyAssignmentId;
            ItemPositionId = itemPositionId;
            SourcePositionId = sourcePositionId;
            TargetPositionId = targetPositionId;
            Quantity = quantity;
            Status = OrderAssemblyLineStatus.Pending;
        }

        public void MarkAsPicked()
        {
            if (Status != OrderAssemblyLineStatus.Pending)
                throw new InvalidOperationException("Item can only be picked if it is pending.");
            Status = OrderAssemblyLineStatus.Picked;
        }

        public void MarkAsPlaced()
        {
            if (Status != OrderAssemblyLineStatus.Picked)
                throw new InvalidOperationException("Item can only be placed after it is picked.");
            Status = OrderAssemblyLineStatus.Placed;
        }

        public void ReportDiscrepancy()
        {
            Status = OrderAssemblyLineStatus.Discrepancy;
        }
    }
}
