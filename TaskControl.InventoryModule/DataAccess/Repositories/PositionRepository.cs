using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Mapper;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DAL.Repositories
{
    public class PositionCellRepository : IRepository<PositionCell>
    {
        private readonly IInventoryDataConnection _db;
        private readonly ILogger<PositionCellRepository> _logger;

        public PositionCellRepository(IInventoryDataConnection db, ILogger<PositionCellRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<PositionCell?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск ячейки по ID: {id}", id);
            try
            {
                var position = await _db.PositionCells.FirstOrDefaultAsync(p => p.PositionId == id);
                return position?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении ячейки по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PositionCell>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех ячеек");
            try
            {
                var positionsModel = await _db.PositionCells.ToListAsync();
                return positionsModel.Select(p => p.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка ячеек");
                throw;
            }
        }

        public async Task<int> AddAsync(PositionCell entity)
        {
            _logger.LogInformation("Добавление новой ячейки ID: {id}", entity.PositionId);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var model = entity.ToModel();
                return await _db.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении ячейки ID: {id}", entity?.PositionId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(PositionCell entity)
        {
            _logger.LogInformation("Обновление ячейки ID: {id}", entity.PositionId);
            try
            {
                if (entity == null)
                    return 0;

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении ячейки ID: {id}", entity?.PositionId);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление ячейки ID: {id}", id);
            try
            {
                var position = await _db.PositionCells.FirstOrDefaultAsync(p => p.PositionId == id);
                if (position is null)
                    return 0;

                return await _db.DeleteAsync(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении ячейки ID: {id}", id);
                throw;
            }
        }
    }
}