using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Interface
{
    public interface IActiveTaskRepository
    {
        Task<ActiveTask?> GetByIdAsync(int id);
        Task<IEnumerable<ActiveTask>> GetAllAsync();
        Task<int> AddAsync(ActiveTask entity);
        Task<int> UpdateAsync(ActiveTask entity);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<ActiveTask>> GetByBranchAsync(int branchId);
        Task<IEnumerable<ActiveTask>> GetByStatusAsync(string status);
        Task<IEnumerable<ActiveTask>> GetActiveTasksAsync();
        Task<IEnumerable<ActiveTask>> GetByTypeAsync(string type);
    }
}