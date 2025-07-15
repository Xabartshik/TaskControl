using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Mapper;
using TaskControl.OrderModule.Domain;

namespace TaskControl.InventoryModule.DAL.Repositories
{
    public class ItemStatusRepository : IRepository<ItemStatus>
    {
        private readonly IInventoryDataConnection _db;
        private readonly ILogger<ItemStatusRepository> _logger;

        public ItemStatusRepository(IInventoryDataConnection db, ILogger<ItemStatusRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<ItemStatus?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск статуса товара по ID: {id}", id);
            try
            {
                var status = await _db.ItemStatuses.FirstOrDefaultAsync(s => s.Id == id);
                return status?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статуса товара по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ItemStatus>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех статусов товаров");
            try
            {
                var statuses = await _db.ItemStatuses.ToListAsync();
                return statuses.Select(s => s.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка статусов товаров");
                throw;
            }
        }

        public async Task<int> AddAsync(ItemStatus entity)
        {
            _logger.LogInformation("Добавление нового статуса товара для позиции {itemPositionId}", entity.ItemPositionId);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Устанавливаем текущую дату, если не задана
                if (entity.StatusDate == default)
                    entity.StatusDate = DateTime.UtcNow;

                var model = entity.ToModel();
                return await _db.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении статуса товара для позиции {itemPositionId}", entity?.ItemPositionId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(ItemStatus entity)
        {
            _logger.LogInformation("Обновление статуса товара ID: {id}", entity.Id);
            try
            {
                if (entity == null)
                    return 0;

                // Обновляем дату статуса
                entity.StatusDate = DateTime.UtcNow;

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении статуса товара ID: {id}", entity?.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление статуса товара ID: {id}", id);
            try
            {
                var status = await _db.ItemStatuses.FirstOrDefaultAsync(s => s.Id == id);
                if (status is null)
                    return 0;

                return await _db.DeleteAsync(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении статуса товара ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ItemStatus>> GetByItemPositionIdAsync(int itemPositionId)
        {
            _logger.LogInformation("Получение статусов для товарной позиции ID: {itemPositionId}", itemPositionId);
            try
            {
                var statuses = await _db.ItemStatuses
                    .Where(s => s.ItemPositionId == itemPositionId)
                    .OrderByDescending(s => s.StatusDate)
                    .ToListAsync();

                return statuses.Select(s => s.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статусов для товарной позиции ID: {itemPositionId}", itemPositionId);
                throw;
            }
        }

        public async Task<ItemStatus?> GetCurrentStatusAsync(int itemPositionId)
        {
            _logger.LogInformation("Получение текущего статуса для товарной позиции ID: {itemPositionId}", itemPositionId);
            try
            {
                var status = await _db.ItemStatuses
                    .Where(s => s.ItemPositionId == itemPositionId)
                    .OrderByDescending(s => s.StatusDate)
                    .FirstOrDefaultAsync();

                return status?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении текущего статуса для товарной позиции ID: {itemPositionId}", itemPositionId);
                throw;
            }
        }
    }
}