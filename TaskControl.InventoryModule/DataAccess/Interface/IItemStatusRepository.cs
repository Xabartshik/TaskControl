using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.OrderModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Interface
{
    public interface IItemStatusRepository
    {
        Task<ItemStatus?> GetByIdAsync(int id);
        Task<IEnumerable<ItemStatus>> GetAllAsync();
        Task<int> AddAsync(ItemStatus entity);
        Task<int> UpdateAsync(ItemStatus entity);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<ItemStatus>> GetByItemPositionIdAsync(int itemPositionId);
        Task<ItemStatus?> GetCurrentStatusAsync(int itemPositionId);
    }
}