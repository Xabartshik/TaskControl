using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Domain;

namespace TaskControl.ReportsModule.DataAccess.Interface
{
    public interface IRawEventRepository
    {
        Task<RawEvent?> GetByIdAsync(int id);
        Task<IEnumerable<RawEvent>> GetAllAsync();
        Task<int> AddAsync(RawEvent entity);
        Task<int> UpdateAsync(RawEvent entity);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<RawEvent>> GetByTypeAsync(string type);
        Task<IEnumerable<RawEvent>> GetByTimeRangeAsync(DateTime from, DateTime to);
    }
}