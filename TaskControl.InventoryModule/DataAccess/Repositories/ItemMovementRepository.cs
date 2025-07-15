using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Mapper;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DAL.Repositories
{
    public class ItemMovementRepository : IRepository<ItemMovement>, IItemMovementRepository
    {
        private readonly IInventoryDataConnection _db;
        private readonly ILogger<ItemMovementRepository> _logger;

        public ItemMovementRepository(IInventoryDataConnection db, ILogger<ItemMovementRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<ItemMovement?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск перемещения товара по ID: {id}", id);
            try
            {
                var movement = await _db.ItemMovements.FirstOrDefaultAsync(m => m.Id == id);
                return movement?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении перемещения товара по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ItemMovement>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех перемещений товаров");
            try
            {
                var movements = await _db.ItemMovements.ToListAsync();
                return movements.Select(m => m.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка перемещений товаров");
                throw;
            }
        }

        public async Task<int> AddAsync(ItemMovement entity)
        {
            _logger.LogInformation("Добавление нового перемещения товара");
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
                _logger.LogError(ex, "Ошибка при добавлении перемещения товара");
                throw;
            }
        }

        public async Task<int> UpdateAsync(ItemMovement entity)
        {
            _logger.LogInformation("Обновление перемещения товара ID: {id}", entity.Id);
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
                _logger.LogError(ex, "Ошибка при обновлении перемещения товара ID: {id}", entity?.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление перемещения товара ID: {id}", id);
            try
            {
                var movement = await _db.ItemMovements.FirstOrDefaultAsync(m => m.Id == id);
                if (movement is null)
                    return 0;

                return await _db.DeleteAsync(movement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении перемещения товара ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ItemMovement>> GetBySourcePositionAsync(int itemPositionId)
        {
            _logger.LogInformation("Получение перемещений из позиции ID: {itemPositionId}", itemPositionId);
            try
            {
                var movements = await _db.ItemMovements
                    .Where(m => m.SourceItemPositionId == itemPositionId)
                    .ToListAsync();

                return movements.Select(m => m.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении перемещений из позиции ID: {itemPositionId}", itemPositionId);
                throw;
            }
        }

        public async Task<IEnumerable<ItemMovement>> GetByDestinationPositionAsync(int itemPositionId)
        {
            _logger.LogInformation("Получение перемещений в позицию ID: {itemPositionId}", itemPositionId);
            try
            {
                var movements = await _db.ItemMovements
                    .Where(m => m.DestinationPositionId == itemPositionId)
                    .ToListAsync();

                return movements.Select(m => m.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении перемещений в позицию ID: {itemPositionId}", itemPositionId);
                throw;
            }
        }

        public async Task<IEnumerable<ItemMovement>> GetByBranchAsync(int branchId)
        {
            _logger.LogInformation("Получение перемещений по филиалу ID: {branchId}", branchId);
            try
            {
                var movements = await _db.ItemMovements
                    .Where(m => m.SourceBranchId == branchId || m.DestinationBranchId == branchId)
                    .ToListAsync();

                return movements.Select(m => m.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении перемещений по филиалу ID: {branchId}", branchId);
                throw;
            }
        }
    }
}