using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Mapper;
using TaskControl.OrderModule.Domain;

namespace TaskControl.InventoryModule.DAL.Repositories
{
    public class OrderPositionRepository : IRepository<OrderPosition>, IOrderPositionRepository
    {
        private readonly IInventoryDataConnection _db;
        private readonly ILogger<OrderPositionRepository> _logger;

        public OrderPositionRepository(IInventoryDataConnection db, ILogger<OrderPositionRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<OrderPosition?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск позиции заказа по ID: {id}", id);
            try
            {
                var orderPosition = await _db.OrderPositions.FirstOrDefaultAsync(op => op.UniqueId == id);
                return orderPosition?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении позиции заказа по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<OrderPosition>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех позиций заказов");
            try
            {
                var orderPositions = await _db.OrderPositions.ToListAsync();
                return orderPositions.Select(op => op.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка позиций заказов");
                throw;
            }
        }

        public async Task<int> AddAsync(OrderPosition entity)
        {
            _logger.LogInformation("Добавление новой позиции в заказ {orderId}", entity.OrderId);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (entity.Quantity <= 0)
                    throw new ArgumentException("Количество должно быть положительным", nameof(entity.Quantity));

                var model = entity.ToModel();
                return await _db.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении позиции в заказ {orderId}", entity?.OrderId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(OrderPosition entity)
        {
            _logger.LogInformation("Обновление позиции заказа ID: {id}", entity.UniqueId);
            try
            {
                if (entity == null)
                    return 0;

                if (entity.Quantity <= 0)
                    throw new ArgumentException("Количество должно быть положительным", nameof(entity.Quantity));

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении позиции заказа ID: {id}", entity?.UniqueId);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление позиции заказа ID: {id}", id);
            try
            {
                var orderPosition = await _db.OrderPositions.FirstOrDefaultAsync(op => op.UniqueId == id);
                if (orderPosition is null)
                    return 0;

                return await _db.DeleteAsync(orderPosition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении позиции заказа ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<OrderPosition>> GetByOrderIdAsync(int orderId)
        {
            _logger.LogInformation("Получение позиций для заказа ID: {orderId}", orderId);
            try
            {
                var orderPositions = await _db.OrderPositions
                    .Where(op => op.OrderId == orderId)
                    .ToListAsync();

                return orderPositions.Select(op => op.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении позиций для заказа ID: {orderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<OrderPosition>> GetByItemPositionIdAsync(int itemPositionId)
        {
            _logger.LogInformation("Получение заказов для товарной позиции ID: {itemPositionId}", itemPositionId);
            try
            {
                var orderPositions = await _db.OrderPositions
                    .Where(op => op.ItemPositionId == itemPositionId)
                    .ToListAsync();

                return orderPositions.Select(op => op.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказов для товарной позиции ID: {itemPositionId}", itemPositionId);
                throw;
            }
        }
    }
}