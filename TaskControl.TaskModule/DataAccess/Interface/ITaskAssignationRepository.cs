using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Interface
{
    public interface ITaskAssignationRepository
    {
        Task<TaskAssignation?> GetByIdAsync(int id);
        Task<IEnumerable<TaskAssignation>> GetAllAsync();
        Task<int> AddAsync(TaskAssignation entity);
        Task<int> UpdateAsync(TaskAssignation entity);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<TaskAssignation>> GetByTaskIdAsync(int taskId);
        Task<IEnumerable<TaskAssignation>> GetByUserIdAsync(int userId);
        Task<TaskAssignation?> GetCurrentAssignmentAsync(int taskId);
    }
}