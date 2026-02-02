using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Interface
{
    public interface IItemPositionRepository
    {
        Task<ItemPosition?> GetByIdAsync(int id);
        Task<IEnumerable<ItemPosition>> GetByIdsAsync(IEnumerable<int> ids);
        Task<IEnumerable<ItemPosition>> GetAllAsync();
        Task<int> AddAsync(ItemPosition entity);
        Task<int> UpdateAsync(ItemPosition entity);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<ItemPosition>> GetByItemIdAsync(int itemId);
        Task<IEnumerable<ItemPosition>> GetByPositionIdAsync(int positionId);
    }
}