using System;

namespace TaskControl.TaskModule.Domain
{
    public abstract class WorkerAssignment
    {
        public int Id { get; internal set; }
        public int TaskId { get; internal set; }
        public int AssignedToUserId { get; internal set; }
        public int BranchId { get; internal set; }

        public AssignmentStatus Status { get; internal set; }

        public DateTime AssignedAt { get; internal set; }
        public DateTime? StartedAt { get; internal set; }
        public DateTime? CompletedAt { get; internal set; }

        public bool IsCompleted => Status == AssignmentStatus.Completed;

        public WorkerAssignment() { }

        // 2. Конструктор для существующих записей (6 аргументов)
        public WorkerAssignment(
            int id,
            int taskId,
            int assignedToUserId,
            int branchId,
            AssignmentStatus status,
            DateTime assignedAtUtc)
        {
            Id = id;
            TaskId = taskId;
            AssignedToUserId = assignedToUserId;
            BranchId = branchId;
            Status = status;
            AssignedAt = assignedAtUtc;
        }

        // 3. Конструктор для новых назначений (4 аргумента)
        public WorkerAssignment(
            int taskId,
            int assignedToUserId,
            int branchId,
            DateTime assignedAtUtc = default)
        {
            TaskId = taskId;
            AssignedToUserId = assignedToUserId;
            BranchId = branchId;
            AssignedAt = assignedAtUtc == default ? DateTime.UtcNow : assignedAtUtc;
            Status = AssignmentStatus.Assigned;
        }

        public virtual void Start(DateTime startedAtUtc)
        {
            if (Status == AssignmentStatus.Cancelled || Status == AssignmentStatus.Completed)
                throw new InvalidOperationException("Нельзя начать завершенное или отмененное назначение.");

            Status = AssignmentStatus.InProgress;
            // Если задача запускается впервые, фиксируем время. При снятии с паузы время старта не перезаписываем
            StartedAt ??= startedAtUtc;
        }

        public virtual void Pause()
        {
            if (Status != AssignmentStatus.InProgress)
                throw new InvalidOperationException("Можно поставить на паузу только активное назначение.");

            Status = AssignmentStatus.Paused;
        }

        public virtual void Complete(DateTime completedAtUtc)
        {
            if (Status == AssignmentStatus.Cancelled)
                throw new InvalidOperationException("Нельзя завершить отмененное назначение.");

            Status = AssignmentStatus.Completed;
            CompletedAt = completedAtUtc;
        }

        public virtual void Cancel()
        {
            if (Status == AssignmentStatus.Completed)
                throw new InvalidOperationException("Нельзя отменить завершенное назначение.");

            Status = AssignmentStatus.Cancelled;
        }
    }
}