using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Interface
{
    public interface IOrderAssemblyAssignmentRepository
    {
        Task<OrderAssemblyAssignment> GetByIdAsync(int id);
        Task<OrderAssemblyAssignment> GetByTaskIdAsync(int taskId);
        Task<List<OrderAssemblyAssignment>> GetByUserIdAsync(int userId);
        Task<List<OrderAssemblyAssignment>> GetByStatusAsync(AssignmentStatus status);
        Task<List<OrderAssemblyAssignment>> GetByBranchIdAsync(int branchId);
        Task<int> AddAsync(OrderAssemblyAssignment assignment);
        Task<int> UpdateAsync(OrderAssemblyAssignment assignment);
        Task<OrderAssemblyAssignmentModel> GetByTaskAndUserAsync(int taskId, int workerId);
        Task<List<OrderAssemblyAssignmentModel>> GetAllByTaskIdAsync(int taskId);
    }
}
