using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.DTOs;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.Application.Interface
{
    public interface IBaseTaskService : IService<BaseTaskDto>
    {
        Task<IEnumerable<int>> GetAutoSelectedEmployeesAsync(int branchId, int requiredCount);
        Task<bool> UpdateTaskStatusAsync(int taskId, TaskStatus newStatus);
    }
}
