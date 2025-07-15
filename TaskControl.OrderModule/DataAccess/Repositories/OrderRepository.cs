using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.SharedInterfaces;
using TaskControl.OrderModule.DataAccess.Interface;
using TaskControl.OrderModule.DataAccess.Mapper;
using TaskControl.OrderModule.Domain;

namespace TaskControl.OrderModule.DAL.Repositories
{
    public class OrderRepository : IRepository<Order>, IOrderRepository
    {
        private readonly IOrderDataConnection _db;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(IOrderDataConnection db, ILogger<OrderRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск заказа по ID: {id}", id);
            try
            {
                var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
                return order?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказа по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех заказов");
            try
            {
                var ordersModel = await _db.Orders.ToListAsync();
                return ordersModel.Select(o => o.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка заказов");
                throw;
            }
        }

        public async Task<int> AddAsync(Order entity)
        {
            _logger.LogInformation("Добавление нового заказа для клиента {customerId}", entity.CustomerId);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Валидация типа заказа
                if (!new[] { "Online", "Offline" }.Contains(entity.Type))
                    throw new ArgumentException("Недопустимый тип заказа");

                var model = entity.ToModel();
                return await _db.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении заказа для клиента {customerId}", entity?.CustomerId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Order entity)
        {
            _logger.LogInformation("Обновление заказа ID: {orderId}", entity.OrderId);
            try
            {
                if (entity == null)
                    return 0;

                // Валидация статуса заказа
                if (!new[] { "New", "Processing", "Delivered", "Cancelled" }.Contains(entity.Status))
                    throw new ArgumentException("Недопустимый статус заказа");

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении заказа ID: {orderId}", entity?.OrderId);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление заказа ID: {id}", id);
            try
            {
                var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
                if (order is null)
                    return 0;

                return await _db.DeleteAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении заказа ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetByCustomerAsync(int customerId)
        {
            _logger.LogInformation("Получение заказов клиента ID: {customerId}", customerId);
            try
            {
                var orders = await _db.Orders
                    .Where(o => o.CustomerId == customerId)
                    .ToListAsync();

                return orders.Select(o => o.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказов клиента ID: {customerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetByBranchAsync(int branchId)
        {
            _logger.LogInformation("Получение заказов филиала ID: {branchId}", branchId);
            try
            {
                var orders = await _db.Orders
                    .Where(o => o.BranchId == branchId)
                    .ToListAsync();

                return orders.Select(o => o.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказов филиала ID: {branchId}", branchId);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
        {
            _logger.LogInformation("Получение заказов со статусом: {status}", status);
            try
            {
                var orders = await _db.Orders
                    .Where(o => o.Status == status)
                    .ToListAsync();

                return orders.Select(o => o.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказов со статусом: {status}", status);
                throw;
            }
        }
    }
}