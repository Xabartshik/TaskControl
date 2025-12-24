using TaskControl.InventoryModule.Domain;

namespace TaskControl.TaskModule.Domain
{
    public enum InventoryAssignmentStatus
    {
        Assigned = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }

    /// <summary>
    /// Строка назначения инвентаризации: конкретный товар в конкретной ячейке.
    /// </summary>
    public class InventoryAssignmentLine
    {
        public int Id { get; internal set; }

        /// <summary>
        /// Ссылка на назначение (родительский агрегат).
        /// </summary>
        public int InventoryAssignmentId { get; internal set; }

        /// <summary>
        /// Id ItemPosition (физическое расположение товара на складе).
        /// </summary>
        public int ItemPositionId { get; internal set; }

        /// <summary>
        /// Id складской ячейки (PositionCell.PositionId).
        /// </summary>
        public int PositionId { get; internal set; }

        /// <summary>
        /// Снимок кода позиции (адрес ячейки) на момент назначения.
        /// </summary>
        public PositionCode PositionCode { get; internal set; }

        /// <summary>
        /// Ожидаемое количество (до инвентаризации).
        /// </summary>
        public int ExpectedQuantity { get; internal set; }

        /// <summary>
        /// Фактически отсканированное количество.
        /// </summary>
        public int? ActualQuantity { get; internal set; }

        public bool IsCounted => ActualQuantity.HasValue;

        internal InventoryAssignmentLine() { }

        public InventoryAssignmentLine(
            int id,
            int inventoryAssignmentId,
            int itemPositionId,
            int positionId,
            PositionCode positionCode,
            int expectedQuantity)
        {
            if (inventoryAssignmentId <= 0)
                throw new ArgumentOutOfRangeException(nameof(inventoryAssignmentId));
            if (itemPositionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(itemPositionId));
            if (positionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(positionId));
            if (expectedQuantity < 0)
                throw new ArgumentOutOfRangeException(nameof(expectedQuantity));
            if (positionCode == null)
                throw new ArgumentNullException(nameof(positionCode));

            Id = id;
            InventoryAssignmentId = inventoryAssignmentId;
            ItemPositionId = itemPositionId;
            PositionId = positionId;
            PositionCode = positionCode;
            ExpectedQuantity = expectedQuantity;
        }

        public InventoryAssignmentLine(
            int inventoryAssignmentId,
            int itemPositionId,
            int positionId,
            PositionCode positionCode,
            int expectedQuantity)
        {
            if (inventoryAssignmentId <= 0)
                throw new ArgumentOutOfRangeException(nameof(inventoryAssignmentId));
            if (itemPositionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(itemPositionId));
            if (positionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(positionId));
            if (expectedQuantity < 0)
                throw new ArgumentOutOfRangeException(nameof(expectedQuantity));
            if (positionCode == null)
                throw new ArgumentNullException(nameof(positionCode));

            InventoryAssignmentId = inventoryAssignmentId;
            ItemPositionId = itemPositionId;
            PositionId = positionId;
            PositionCode = positionCode;
            ExpectedQuantity = expectedQuantity;
        }

        public void SetActualQuantity(int actualQuantity)
        {
            if (actualQuantity < 0)
                throw new ArgumentOutOfRangeException(nameof(actualQuantity));

            ActualQuantity = actualQuantity;
        }
    }

    /// <summary>
    /// Назначение участка инвентаризации конкретному работнику.
    /// </summary>
    public class InventoryAssignment
    {
        public int Id { get; internal set; }

        /// <summary>
        /// Id задачи (Task из TaskModule) с типом TaskType.Inventory.
        /// </summary>
        public int TaskId { get; internal set; }

        /// <summary>
        /// Пользователь, которому назначен участок.
        /// </summary>
        public int AssignedToUserId { get; internal set; }

        /// <summary>
        /// Филиал (BranchId из PositionCell). Для всего назначения.
        /// </summary>
        public int BranchId { get; internal set; }

        /// <summary>
        /// Необязательный код зоны (ZoneCode из PositionCell) для всего назначения.
        /// </summary>
        public string? ZoneCode { get; internal set; }

        public InventoryAssignmentStatus Status { get; internal set; }

        public DateTime AssignedAt { get; internal set; }
        public DateTime? CompletedAt { get; internal set; }

        internal readonly List<InventoryAssignmentLine> _lines = new();
        public IReadOnlyCollection<InventoryAssignmentLine> Lines => _lines.AsReadOnly();

        public int TotalLines => _lines.Count;
        public int CountedLines => _lines.Count(l => l.IsCounted);
        public bool IsCompleted => Status == InventoryAssignmentStatus.Completed;

        public InventoryAssignment() { }

        public InventoryAssignment(
            int id,
            int taskId,
            int assignedToUserId,
            int branchId,
            DateTime assignedAtUtc,
            IEnumerable<InventoryAssignmentLine> lines)
        {
            if (taskId <= 0)
                throw new ArgumentOutOfRangeException(nameof(taskId));
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
            AssignedToUserId = assignedToUserId;
            BranchId = branchId;
            AssignedAt = assignedAtUtc;
            Status = InventoryAssignmentStatus.Assigned;

            _lines.AddRange(lineList);
        }

        public InventoryAssignment(
            int taskId,
            int assignedToUserId,
            int branchId,
            DateTime assignedAtUtc = default)
        {
            if (taskId <= 0) throw new ArgumentOutOfRangeException(nameof(taskId));
            if (assignedToUserId <= 0) throw new ArgumentOutOfRangeException(nameof(assignedToUserId));
            if (branchId <= 0) throw new ArgumentOutOfRangeException(nameof(branchId));

            TaskId = taskId;
            AssignedToUserId = assignedToUserId;
            BranchId = branchId;
            AssignedAt = assignedAtUtc == default ? DateTime.UtcNow : assignedAtUtc;
            Status = InventoryAssignmentStatus.Assigned;
        }


        public void Start()
        {
            if (Status == InventoryAssignmentStatus.Cancelled ||
                Status == InventoryAssignmentStatus.Completed)
                throw new InvalidOperationException("Cannot start completed or cancelled assignment.");

            Status = InventoryAssignmentStatus.InProgress;
        }

        public void Complete(DateTime completedAtUtc)
        {
            if (Status == InventoryAssignmentStatus.Cancelled)
                throw new InvalidOperationException("Cannot complete cancelled assignment.");

            Status = InventoryAssignmentStatus.Completed;
            CompletedAt = completedAtUtc;
        }

        public void Cancel()
        {
            if (Status == InventoryAssignmentStatus.Completed)
                throw new InvalidOperationException("Cannot cancel completed assignment.");

            Status = InventoryAssignmentStatus.Cancelled;
        }

        /// <summary>
        /// Обновить фактическое количество для конкретного ItemPosition.
        /// </summary>
        public void SetActualQuantityForItemPosition(int itemPositionId, int actualQuantity)
        {
            var line = _lines.SingleOrDefault(l => l.ItemPositionId == itemPositionId);
            if (line == null)
                throw new InvalidOperationException("ItemPosition is not part of this assignment.");

            line.SetActualQuantity(actualQuantity);
        }

        /// <summary>
        /// Проверить, относится ли складская позиция (PositionId) к этому назначению.
        /// </summary>
        public bool ContainsPosition(int positionId)
        {
            return _lines.Any(l => l.PositionId == positionId);
        }

        /// <summary>
        /// Проверить, относится ли позиция с заданным кодом к этому назначению.
        /// Удобно использовать при работе с QR, содержащим строковый код.
        /// </summary>
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
