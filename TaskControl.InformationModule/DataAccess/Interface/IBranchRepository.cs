using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Interface
{
    public interface IBranchRepository
    {
        Task<Branch?> GetByIdAsync(int id);
        Task<IEnumerable<Branch>> GetAllAsync();
        Task<int> AddAsync(Branch entity);
        Task<int> UpdateAsync(Branch entity);
        Task<int> DeleteAsync(int id);
    }
}