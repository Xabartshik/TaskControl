using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.Domain;
using TaskControl.OrderModule.DataAccess.Model; // Обязательно добавьте эту ссылку

namespace TaskControl.InventoryModule.DAL.Repositories
{
    public interface IOrderReservationRepository
    {
        Task<int> AddAsync(OrderReservation reservation);
        Task<decimal> GetReservedQuantityInBranchAsync(int itemId, int branchId);
    }

    public class OrderReservationRepository : IOrderReservationRepository
    {
        private readonly IInventoryDataConnection _db;
        private readonly ILogger<OrderReservationRepository> _logger;

        public OrderReservationRepository(IInventoryDataConnection db, ILogger<OrderReservationRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> AddAsync(OrderReservation reservation)
        {
            var model = new OrderReservationModel
            {
                OrderPositionId = reservation.OrderPositionId,
                // Поле ItemPositionId теперь может принимать null (int?)
                ItemPositionId = reservation.ItemPositionId,
                Quantity = reservation.Quantity,
                CreatedAt = DateTime.UtcNow
            };
            return await _db.InsertAsync(model);
        }

        public async Task<decimal> GetReservedQuantityInBranchAsync(int itemId, int branchId)
        {
            // Используем GetTable<OrderModel>(), так как таблица заказов находится в другом модуле
            // Приводим Quantity к decimal?, чтобы SumAsync корректно обрабатывал пустые результаты
            var query = from r in _db.GetTable<OrderReservationModel>()
                        join op in _db.OrderPositions on r.OrderPositionId equals op.UniqueId
                        join o in _db.GetTable<OrderModel>() on op.OrderId equals o.OrderId
                        where op.ItemId == itemId && o.BranchId == branchId
                        select (decimal?)r.Quantity;

            var totalReserved = await query.SumAsync();
            return totalReserved ?? 0m;
        }
    }
}