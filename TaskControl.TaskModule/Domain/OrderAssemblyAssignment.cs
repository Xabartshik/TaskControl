using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskControl.TaskModule.Domain
{
    public enum OrderAssemblyAssignmentStatus
    {
        Assigned = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }

    /// <summary>
    /// Назначение сборки заказа конкретному работнику.
    /// </summary>
    public class OrderAssemblyAssignment
    {
        public int Id { get; internal set; }

        /// <summary>
        /// Id задачи (Task из TaskModule) с типом Type = 'OrderAssembly'.
        /// </summary>
        public int TaskId { get; internal set; }

        /// <summary>
        /// Id собираемого заказа.
        /// </summary>
        public int OrderId { get; internal set; }

        /// <summary>
        /// Пользователь, которому назначена сборка.
        /// </summary>
        public int AssignedToUserId { get; internal set; }

        /// <summary>
        /// Филиал.
        /// </summary>
        public int BranchId { get; internal set; }

        public OrderAssemblyAssignmentStatus Status { get; internal set; }

        public DateTime AssignedAt { get; internal set; }
        public DateTime? StartedAt { get; internal set; }
        public DateTime? CompletedAt { get; internal set; }

        internal readonly List<OrderAssemblyLine> _lines = new();
        public IReadOnlyCollection<OrderAssemblyLine> Lines => _lines.AsReadOnly();

        public int TotalLines => _lines.Count;
        public bool IsCompleted => Status == OrderAssemblyAssignmentStatus.Completed;

        public OrderAssemblyAssignment() { }

        public OrderAssemblyAssignment(
            int id,
            int taskId,
            int orderId,
            int assignedToUserId,
            int branchId,
            OrderAssemblyAssignmentStatus status,
            DateTime assignedAtUtc,
            IEnumerable<OrderAssemblyLine> lines)
        {
            if (taskId <= 0)
                throw new ArgumentOutOfRangeException(nameof(taskId));
            if (orderId <= 0)
                throw new ArgumentOutOfRangeException(nameof(orderId));
            if (assignedToUserId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assignedToUserId));
            if (branchId <= 0)
                throw new ArgumentOutOfRangeException(nameof(branchId));
            if (lines == null)
                throw new ArgumentNullException(nameof(lines));

            var lineList = lines.ToList();
            if (lineList.Count == 0)
                throw new ArgumentException("At least one line must be provided.", nameof(lines));

            Id = id;
            TaskId = taskId;
            OrderId = orderId;
            AssignedToUserId = assignedToUserId;
            BranchId = branchId;
            AssignedAt = assignedAtUtc;
            Status = status;

            _lines.AddRange(lineList);
        }

        public OrderAssemblyAssignment(
            int taskId,
            int orderId,
            int assignedToUserId,
            int branchId,
            DateTime assignedAtUtc = default)
        {
            if (taskId <= 0) throw new ArgumentOutOfRangeException(nameof(taskId));
            if (orderId <= 0) throw new ArgumentOutOfRangeException(nameof(orderId));
            if (assignedToUserId <= 0) throw new ArgumentOutOfRangeException(nameof(assignedToUserId));
            if (branchId <= 0) throw new ArgumentOutOfRangeException(nameof(branchId));

            TaskId = taskId;
            OrderId = orderId;
            AssignedToUserId = assignedToUserId;
            BranchId = branchId;
            AssignedAt = assignedAtUtc == default ? DateTime.UtcNow : assignedAtUtc;
            Status = OrderAssemblyAssignmentStatus.Assigned;
        }

        public void AddLine(OrderAssemblyLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            _lines.Add(line);
        }

        public void Start(DateTime startedAtUtc)
        {
            if (Status == OrderAssemblyAssignmentStatus.Cancelled ||
                Status == OrderAssemblyAssignmentStatus.Completed)
                throw new InvalidOperationException("Cannot start completed or cancelled assignment.");

            Status = OrderAssemblyAssignmentStatus.InProgress;
            StartedAt = startedAtUtc;
        }

        public void Complete(DateTime completedAtUtc)
        {
            if (Status == OrderAssemblyAssignmentStatus.Cancelled)
                throw new InvalidOperationException("Cannot complete cancelled assignment.");

            Status = OrderAssemblyAssignmentStatus.Completed;
            CompletedAt = completedAtUtc;
        }

        public void Cancel()
        {
            if (Status == OrderAssemblyAssignmentStatus.Completed)
                throw new InvalidOperationException("Cannot cancel completed assignment.");

            Status = OrderAssemblyAssignmentStatus.Cancelled;
        }
    }
}
