using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Mapper;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.Domain;


namespace TaskControl.InventoryModule.DataAccess.Repositories
{
    public class PostamatCellRepository : IRepository<PostamatCell>, IPostamatCellRepository
    {
        private readonly IInventoryDataConnection _db;
        private readonly ILogger<PostamatCellRepository> _logger;

        public PostamatCellRepository(IInventoryDataConnection db, ILogger<PostamatCellRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<PostamatCell> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск ячейки постамата по ID: {id}", id);
            try
            {
                var cell = await _db.GetTable<PostamatCellModel>().FirstOrDefaultAsync(c => c.CellId == id);
                return cell?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении ячейки постамата по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PostamatCell>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех ячеек постаматов");
            try
            {
                var cells = await _db.GetTable<PostamatCellModel>().ToListAsync();
                return cells.Select(c => c.ToDomain()!).Where(c => c != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех ячеек постаматов");
                throw;
            }
        }

        public async Task<IEnumerable<PostamatCell>> GetCellsByPostamatIdAsync(int postamatId)
        {
            _logger.LogInformation("Получение ячеек для постамата ID: {id}", postamatId);
            try
            {
                var cells = await _db.GetTable<PostamatCellModel>()
                    .Where(c => c.PostamatId == postamatId)
                    .ToListAsync();
                return cells.Select(c => c.ToDomain()!).Where(c => c != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении ячеек для постамата ID: {id}", postamatId);
                throw;
            }
        }

        public async Task<IEnumerable<PostamatCell>> GetAvailableCellsAsync(int postamatId)
        {
            _logger.LogInformation("Получение свободных ячеек для постамата ID: {id}", postamatId);
            try
            {
                var cells = await _db.GetTable<PostamatCellModel>()
                    .Where(c => c.PostamatId == postamatId && c.Status == PostamatCellStatus.Available.ToString())
                    .ToListAsync();
                return cells.Select(c => c.ToDomain()!).Where(c => c != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении свободных ячеек для постамата ID: {id}", postamatId);
                throw;
            }
        }

        public async Task<int> AddAsync(PostamatCell entity)
        {
            _logger.LogInformation("Добавление ячейки для постамата ID: {id}", entity?.PostamatId);
            try
            {
                if (entity == null)
                    return 0;

                var model = entity.ToModel();
                return await _db.InsertWithInt32IdentityAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении ячейки для постамата ID: {id}", entity?.PostamatId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(PostamatCell entity)
        {
            _logger.LogInformation("Обновление ячейки постамата ID: {id}", entity?.CellId);
            try
            {
                if (entity == null)
                    return 0;

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении ячейки постамата ID: {id}", entity?.CellId);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление ячейки постамата ID: {id}", id);
            try
            {
                var cell = await _db.GetTable<PostamatCellModel>().FirstOrDefaultAsync(c => c.CellId == id);
                if (cell is null)
                    return 0;

                return await _db.DeleteAsync(cell);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении ячейки постамата ID: {id}", id);
                throw;
            }
        }
    }
}