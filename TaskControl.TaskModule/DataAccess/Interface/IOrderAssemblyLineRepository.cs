using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Interface
{
    public interface IOrderAssemblyLineRepository
    {
        Task<OrderAssemblyLine> GetByIdAsync(int id);
        Task<List<OrderAssemblyLine>> GetByAssignmentIdAsync(int assignmentId);
        Task<int> AddAsync(OrderAssemblyLine line);
        Task<int> UpdateAsync(OrderAssemblyLine line);
        Task<int> AddBatchAsync(List<OrderAssemblyLine> lines);
    }
}
