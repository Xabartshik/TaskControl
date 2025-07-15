using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Interface
{
    public interface IItemMovementRepository
    {
        Task<ItemMovement?> GetByIdAsync(int id);
        Task<IEnumerable<ItemMovement>> GetAllAsync();
        Task<int> AddAsync(ItemMovement entity);
        Task<int> UpdateAsync(ItemMovement entity);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<ItemMovement>> GetBySourcePositionAsync(int itemPositionId);
        Task<IEnumerable<ItemMovement>> GetByDestinationPositionAsync(int itemPositionId);
        Task<IEnumerable<ItemMovement>> GetByBranchAsync(int branchId);
    }
}