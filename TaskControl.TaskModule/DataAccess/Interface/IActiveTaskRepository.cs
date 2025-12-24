using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.DataAccess.Interface
{
    public interface IActiveTaskRepository
    {
        Task<BaseTask?> GetByIdAsync(int id);
        Task<IEnumerable<BaseTask>> GetAllAsync();
        Task<int> AddAsync(BaseTask entity);
        Task<int> UpdateAsync(BaseTask entity);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<BaseTask>> GetByBranchAsync(int branchId);
        Task<IEnumerable<BaseTask>> GetByStatusAsync(TaskStatus status);
        Task<IEnumerable<BaseTask>> GetActiveTasksAsync();
        Task<IEnumerable<BaseTask>> GetByTypeAsync(string type);
    }
}