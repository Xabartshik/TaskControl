using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskControl.TaskModule.Domain
{
    /// <summary>
    /// Назначение сборки заказа конкретному работнику.
    /// Специализированная задача модуля TaskControl.
    /// </summary>
    public class OrderAssemblyAssignment : WorkerAssignment
    {
        /// <summary>
        /// Id собираемого заказа.
        /// </summary>
        public int OrderId { get; internal set; }

        internal readonly List<OrderAssemblyLine> _lines = new();
        public IReadOnlyCollection<OrderAssemblyLine> Lines => _lines.AsReadOnly();

        public int TotalLines => _lines.Count;

        public OrderAssemblyAssignment() : base() { }

        /// <summary>
        /// Конструктор для загрузки существующего назначения из БД.
        /// </summary>
        public OrderAssemblyAssignment(
            int id,
            int taskId,
            int orderId,
            int assignedToUserId,
            int branchId,
            AssignmentStatus status,
            DateTime assignedAtUtc,
            IEnumerable<OrderAssemblyLine> lines)
            : base(id, taskId, assignedToUserId, branchId, status, assignedAtUtc)
        {
            if (orderId <= 0)
                throw new ArgumentOutOfRangeException(nameof(orderId));

            if (lines == null)
                throw new ArgumentNullException(nameof(lines));

            var lineList = lines.ToList();
            if (lineList.Count == 0)
                throw new ArgumentException("At least one line must be provided.", nameof(lines));

            OrderId = orderId;
            _lines.AddRange(lineList);
        }

        /// <summary>
        /// Конструктор для создания нового назначения.
        /// </summary>
        public OrderAssemblyAssignment(
            int taskId,
            int orderId,
            int assignedToUserId,
            int branchId,
            DateTime assignedAtUtc = default)
            : base(taskId, assignedToUserId, branchId, assignedAtUtc)
        {
            if (orderId <= 0)
                throw new ArgumentOutOfRangeException(nameof(orderId));

            OrderId = orderId;
        }

        public void AddLine(OrderAssemblyLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            _lines.Add(line);
        }

        public override void Start(DateTime startedAtUtc)
        {
            base.Start(startedAtUtc);
        }

        public override void Complete(DateTime completedAtUtc)
        {
            // Здесь можно добавить специфичную для сборки валидацию перед завершением
            if (!_lines.Any())
                throw new InvalidOperationException("Cannot complete assembly without lines.");

            base.Complete(completedAtUtc);
        }

        public override void Cancel()
        {
            base.Cancel();
        }
    }
}