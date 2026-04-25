namespace TaskControl.TaskModule.Application.Interface;

public interface ITaskExecutionAggregator
{
    Task<bool> StartOrResumeTaskAsync(int taskId, int workerId);
}