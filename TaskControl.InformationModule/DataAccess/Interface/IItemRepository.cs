using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Interface
{
    public interface IItemRepository
    {
        Task<Item> GetByIdAsync(int id);
        Task<IEnumerable<Item>> GetAllAsync();
        Task<IEnumerable<Item>> GetByIdsAsync(IEnumerable<int> ids);
        Task<int> AddAsync(Item entity);
        Task<int> UpdateAsync(Item entity);
        Task<int> DeleteAsync(int id);
    }
}
