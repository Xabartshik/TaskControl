using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Mapper;
using TaskControl.InventoryModule.Domain;


namespace TaskControl.InventoryModule.DAL.Repositories
{
    public class ItemPositionRepository : IRepository<ItemPosition>, IItemPositionRepository
    {
        private readonly IInventoryDataConnection _db;
        private readonly ILogger<ItemPositionRepository> _logger;

        public ItemPositionRepository(IInventoryDataConnection db, ILogger<ItemPositionRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<ItemPosition?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск связи товар-позиция по ID: {id}", id);
            try
            {
                var itemPosition = await _db.ItemPositions.FirstOrDefaultAsync(ip => ip.Id == id);
                return itemPosition?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении связи товар-позиция по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ItemPosition>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех связей товар-позиция");
            try
            {
                var itemPositionsModel = await _db.ItemPositions.ToListAsync();
                return itemPositionsModel.Select(ip => ip.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка связей товар-позиция");
                throw;
            }
        }

        public async Task<int> AddAsync(ItemPosition entity)
        {
            _logger.LogInformation("Добавление новой связи товар-позиция");
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var model = entity.ToModel();
                return await _db.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении связи товар-позиция");
                throw;
            }
        }

        public async Task<int> UpdateAsync(ItemPosition entity)
        {
            _logger.LogInformation("Обновление связи товар-позиция ID: {id}", entity.Id);
            try
            {
                if (entity == null)
                    return 0;

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении связи товар-позиция ID: {id}", entity?.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление связи товар-позиция ID: {id}", id);
            try
            {
                var itemPosition = await _db.ItemPositions.FirstOrDefaultAsync(ip => ip.Id == id);
                if (itemPosition is null)
                    return 0;

                return await _db.DeleteAsync(itemPosition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении связи товар-позиция ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ItemPosition>> GetByItemIdAsync(int itemId)
        {
            _logger.LogInformation("Получение позиций для товара ID: {itemId}", itemId);
            try
            {
                var itemPositions = await _db.ItemPositions
                    .Where(ip => ip.ItemId == itemId)
                    .ToListAsync();

                return itemPositions.Select(ip => ip.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении позиций для товара ID: {itemId}", itemId);
                throw;
            }
        }

        public async Task<IEnumerable<ItemPosition>> GetByPositionIdAsync(int positionId)
        {
            _logger.LogInformation("Получение товаров для позиции ID: {positionId}", positionId);
            try
            {
                var itemPositions = await _db.ItemPositions
                    .Where(ip => ip.PositionId == positionId)
                    .ToListAsync();

                return itemPositions.Select(ip => ip.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении товаров для позиции ID: {positionId}", positionId);
                throw;
            }
        }
    }
}