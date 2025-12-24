using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Interface
{
    public interface IOrderPositionRepository
    {
        Task<OrderPosition?> GetByIdAsync(int id);
        Task<IEnumerable<OrderPosition>> GetAllAsync();
        Task<int> AddAsync(OrderPosition entity);
        Task<int> UpdateAsync(OrderPosition entity);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<OrderPosition>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<OrderPosition>> GetByItemPositionIdAsync(int itemPositionId);
    }
}