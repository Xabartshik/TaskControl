using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Interface
{
    public interface IInventoryAssignmentLineRepository
    {
        Task<InventoryAssignmentLine> GetByIdAsync(int id);
        Task<List<InventoryAssignmentLine>> GetByAssignmentIdAsync(int inventoryAssignmentId);
        Task<List<InventoryAssignmentLine>> GetByItemPositionIdAsync(int itemPositionId);
        Task<List<InventoryAssignmentLine>> GetUncountedAsync(int inventoryAssignmentId);
        Task<int> AddAsync(InventoryAssignmentLine line);
        Task<int> UpdateAsync(InventoryAssignmentLine line);
        Task<int> AddBatchAsync(List<InventoryAssignmentLine> lines);
    }
}
