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
        public int OrderAssemblyAssignmentId { get; internal set; }
        public int ItemPositionId { get; internal set; }
        public int SourcePositionId { get; internal set; }
        public int? TargetPositionId { get; internal set; }
        public int Quantity { get; internal set; }
        public int PickedQuantity { get; internal set; }
        public OrderAssemblyLineStatus Status { get; internal set; }

        internal OrderAssemblyLine() { }

        /// <summary>
        /// Конструктор для загрузки существующей строки из БД.
        /// </summary>
        public OrderAssemblyLine(
            int id,
            int orderAssemblyAssignmentId,
            int itemPositionId,
            int sourcePositionId,
            int? targetPositionId,
            int quantity,
            OrderAssemblyLineStatus status)
        {
            if (id < 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (orderAssemblyAssignmentId <= 0) throw new ArgumentOutOfRangeException(nameof(orderAssemblyAssignmentId));
            if (itemPositionId <= 0) throw new ArgumentOutOfRangeException(nameof(itemPositionId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            Id = id;
            OrderAssemblyAssignmentId = orderAssemblyAssignmentId;
            ItemPositionId = itemPositionId;
            SourcePositionId = sourcePositionId;
            TargetPositionId = targetPositionId;
            Quantity = quantity;
            Status = status;
        }

        /// <summary>
        /// Конструктор для создания новой строки.
        /// </summary>
        public OrderAssemblyLine(
            int orderAssemblyAssignmentId,
            int itemPositionId,
            int sourcePositionId,
            int targetPositionId,
            int quantity)
        {
            if (orderAssemblyAssignmentId <= 0) throw new ArgumentOutOfRangeException(nameof(orderAssemblyAssignmentId));
            if (itemPositionId <= 0) throw new ArgumentOutOfRangeException(nameof(itemPositionId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            OrderAssemblyAssignmentId = orderAssemblyAssignmentId;
            ItemPositionId = itemPositionId;
            SourcePositionId = sourcePositionId;
            TargetPositionId = targetPositionId;
            Quantity = quantity;
            PickedQuantity = 0;
            Status = OrderAssemblyLineStatus.Pending;
        }

        public void SetPickedQuantity(int pickedQuantity)
        {
            if (pickedQuantity < 0 || pickedQuantity > Quantity)
                throw new ArgumentOutOfRangeException(nameof(pickedQuantity));

            PickedQuantity = pickedQuantity;
        }

        public void MarkAsPicked()
        {
            if (Status == OrderAssemblyLineStatus.Placed)
                throw new InvalidOperationException("Позиция уже размещена.");

            if (PickedQuantity >= Quantity) return;

            PickedQuantity++;
            if (PickedQuantity >= Quantity)
                Status = OrderAssemblyLineStatus.Picked;
        }

        public void MarkAsPlaced()
        {
            if (Status != OrderAssemblyLineStatus.Picked && Status != OrderAssemblyLineStatus.Placed)
                throw new InvalidOperationException("Разместить позицию можно только после её сборки.");

            Status = OrderAssemblyLineStatus.Placed;
        }

        public void ReportDiscrepancy()
        {
            Status = OrderAssemblyLineStatus.Discrepancy;
        }
    }
}