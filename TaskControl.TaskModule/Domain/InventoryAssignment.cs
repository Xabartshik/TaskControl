using System;
using System.Collections.Generic;
using System.Linq;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.TaskModule.Domain
{
    /// <summary>
    /// Строка назначения инвентаризации: конкретный товар в конкретной ячейке.
    /// </summary>
    public class InventoryAssignmentLine
    {
        public int Id { get; internal set; }
        public int InventoryAssignmentId { get; internal set; }
        public int ItemPositionId { get; internal set; }
        public int PositionId { get; internal set; }
        public PositionCode PositionCode { get; internal set; }
        public int ExpectedQuantity { get; internal set; }
        public int? ActualQuantity { get; internal set; }

        public bool IsCounted => ActualQuantity.HasValue;

        internal InventoryAssignmentLine() { }

        /// <summary>
        /// Конструктор для загрузки существующей строки из БД (с Id).
        /// </summary>
        public InventoryAssignmentLine(
            int id,
            int inventoryAssignmentId,
            int itemPositionId,
            int positionId,
            PositionCode positionCode,
            int expectedQuantity)
        {
            if (id < 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (inventoryAssignmentId <= 0) throw new ArgumentOutOfRangeException(nameof(inventoryAssignmentId));
            if (itemPositionId <= 0) throw new ArgumentOutOfRangeException(nameof(itemPositionId));
            if (positionId <= 0) throw new ArgumentOutOfRangeException(nameof(positionId));
            if (expectedQuantity < 0) throw new ArgumentOutOfRangeException(nameof(expectedQuantity));

            Id = id;
            InventoryAssignmentId = inventoryAssignmentId;
            ItemPositionId = itemPositionId;
            PositionId = positionId;
            PositionCode = positionCode ?? throw new ArgumentNullException(nameof(positionCode));
            ExpectedQuantity = expectedQuantity;
        }

        /// <summary>
        /// Конструктор для создания новой строки (без Id). 
        /// Используется при добавлении неожиданных товаров во время инвентаризации.
        /// </summary>
        public InventoryAssignmentLine(
            int inventoryAssignmentId,
            int itemPositionId,
            int positionId,
            PositionCode positionCode,
            int expectedQuantity)
        {
            if (inventoryAssignmentId <= 0) throw new ArgumentOutOfRangeException(nameof(inventoryAssignmentId));
            if (itemPositionId <= 0) throw new ArgumentOutOfRangeException(nameof(itemPositionId));
            if (positionId <= 0) throw new ArgumentOutOfRangeException(nameof(positionId));
            if (expectedQuantity < 0) throw new ArgumentOutOfRangeException(nameof(expectedQuantity));

            InventoryAssignmentId = inventoryAssignmentId;
            ItemPositionId = itemPositionId;
            PositionId = positionId;
            PositionCode = positionCode ?? throw new ArgumentNullException(nameof(positionCode));
            ExpectedQuantity = expectedQuantity;
        }

        public void SetActualQuantity(int actualQuantity)
        {
            if (actualQuantity < 0) throw new ArgumentOutOfRangeException(nameof(actualQuantity));
            ActualQuantity = actualQuantity;
        }
    }

    /// <summary>
    /// Назначение участка инвентаризации конкретному работнику.
    /// </summary>
    public class InventoryAssignment : WorkerAssignment
    {
        public string? ZoneCode { get; internal set; }

        internal readonly List<InventoryAssignmentLine> _lines = new();
        public IReadOnlyCollection<InventoryAssignmentLine> Lines => _lines.AsReadOnly();

        public int TotalLines => _lines.Count;
        public int CountedLines => _lines.Count(l => l.IsCounted);

        public InventoryAssignment() : base() { }

        /// <summary>
        /// Конструктор для загрузки существующего назначения из БД.
        /// </summary>
        public InventoryAssignment(
            int id,
            int taskId,
            int assignedToUserId,
            int branchId,
            AssignmentStatus status,
            DateTime assignedAtUtc,
            IEnumerable<InventoryAssignmentLine> lines,
            string? zoneCode = null)
            : base(id, taskId, assignedToUserId, branchId, status, assignedAtUtc)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));

            var lineList = lines.ToList();
            if (lineList.Count == 0)
                throw new ArgumentException("По крайней мере одна строка должна быть назначена.", nameof(lines));

            _lines.AddRange(lineList);
            ZoneCode = zoneCode;
        }

        /// <summary>
        /// Конструктор для создания нового назначения.
        /// </summary>
        public InventoryAssignment(
            int taskId,
            int assignedToUserId,
            int branchId,
            DateTime assignedAtUtc = default,
            string? zoneCode = null)
            : base(taskId, assignedToUserId, branchId, assignedAtUtc)
        {
            ZoneCode = zoneCode;
        }

        public override void Complete(DateTime completedAtUtc)
        {
            base.Complete(completedAtUtc);
        }

        public void SetActualQuantityForItemPosition(int itemPositionId, int actualQuantity)
        {
            var line = _lines.SingleOrDefault(l => l.ItemPositionId == itemPositionId);
            if (line == null)
                throw new InvalidOperationException("ItemPosition не входит в это назначение.");

            line.SetActualQuantity(actualQuantity);
        }

        public bool ContainsPosition(int positionId) => _lines.Any(l => l.PositionId == positionId);

        public bool ContainsPositionCode(PositionCode code)
        {
            if (code == null) return false;
            return _lines.Any(l =>
                l.PositionCode.BranchId == code.BranchId &&
                l.PositionCode.ZoneCode == code.ZoneCode &&
                l.PositionCode.FirstLevelStorageType == code.FirstLevelStorageType &&
                l.PositionCode.FLSNumber == code.FLSNumber &&
                l.PositionCode.SecondLevelStorage == code.SecondLevelStorage &&
                l.PositionCode.ThirdLevelStorage == code.ThirdLevelStorage);
        }
    }
}