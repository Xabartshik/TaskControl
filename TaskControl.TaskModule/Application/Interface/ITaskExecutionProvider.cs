namespace TaskControl.TaskModule.Application.Interface;

public interface ITaskExecutionProvider
{
    string TaskType { get; }
    // Ставит на паузу все активные назначения работника в рамках конкретного модуля
    Task PauseActiveTasksAsync(int workerId, int excludeTaskId);
    // Пытается активировать задачу. Возвращает true, если задача принадлежит этому модулю и была успешно переведена в InProgress
    Task<bool> TryActivateTaskAsync(int taskId, int workerId);
    Task<bool> TryPauseTaskAsync(int taskId, int workerId);
    Task<bool> TryCancelTaskAsync(int taskId, int workerId);
    Task<bool> TryCompleteAssignmentAsync(int taskId, int workerId);
    Task<bool> IsTaskFullyCompletedAsync(int taskId);
}
