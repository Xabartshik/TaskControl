using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.OrderModule.Application.DTOs;

namespace TaskControl.OrderModule.Application.Interface
{
    // Наследуем базовый IService, чтобы не терять CRUD
    public interface IOrderService : IService<OrderDto>
    {
        Task<IEnumerable<OrderDto>> GetByCustomerAsync(int customerId);
        Task<bool> CancelOrderAsync(int id);
        Task<IEnumerable<OrderDto>> GetByBranchAsync(int branchId);
        Task<bool> ConfirmPaymentAsync(int id);
        Task CancelUnpaidOrderAsync(int id);
    }
}