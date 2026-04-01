using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Interface
{
    public interface IOrderAssemblyAssignmentRepository
    {
        Task<OrderAssemblyAssignment> GetByIdAsync(int id);
        Task<OrderAssemblyAssignment> GetByTaskIdAsync(int taskId);
        Task<List<OrderAssemblyAssignment>> GetByUserIdAsync(int userId);
        Task<List<OrderAssemblyAssignment>> GetByStatusAsync(OrderAssemblyAssignmentStatus status);
        Task<int> AddAsync(OrderAssemblyAssignment assignment);
        Task<int> UpdateAsync(OrderAssemblyAssignment assignment);
    }
}
