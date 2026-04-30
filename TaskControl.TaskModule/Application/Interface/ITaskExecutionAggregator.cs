namespace TaskControl.TaskModule.Application.Interface;

public interface ITaskExecutionAggregator
{
    Task<bool> CompleteAssignmentAsync(int taskId, string type, int workerId);
    Task<bool> IsTaskFullyCompletedAsync(int taskId, string type);
    Task<bool> StartOrResumeTaskAsync(int taskId, int workerId);
}