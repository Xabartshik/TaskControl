namespace TaskControl.TaskModule.Application.Interface;

public interface ITaskExecutionAggregator
{
    Task StartOrResumeTaskAsync(int taskId, int workerId);
}