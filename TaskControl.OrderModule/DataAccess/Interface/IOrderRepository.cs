using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.OrderModule.Domain;

namespace TaskControl.OrderModule.DataAccess.Interface
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<int> AddAsync(Order entity);
        Task<int> UpdateAsync(Order entity);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<Order>> GetByCustomerAsync(int customerId);
        Task<IEnumerable<Order>> GetByBranchAsync(int branchId);
        Task<IEnumerable<Order>> GetByStatusAsync(string status);
    }
}