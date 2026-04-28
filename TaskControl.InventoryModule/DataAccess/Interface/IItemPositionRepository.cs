using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InformationModule.Domain;
using TaskControl.InventoryModule.Application.DTOs;
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
        Task<IEnumerable<AvailableItemDto>> GetAvailableItemsByBranchAsync(int branchId);
        Task<IEnumerable<AvailableItemDto>> GetAvailableItemsByBranchAsync(int branchId, string search);
        Task<IEnumerable<ItemPosition>> GetByItemAndBranchAsync(int itemId, int branchId);

    }
}