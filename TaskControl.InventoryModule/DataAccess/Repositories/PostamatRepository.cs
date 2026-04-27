using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Mapper;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Repositories
{
    public class PostamatRepository : IRepository<Postamat>, IPostamatRepository
    {
        private readonly IInventoryDataConnection _db;
        private readonly ILogger<PostamatRepository> _logger;

        public PostamatRepository(IInventoryDataConnection db, ILogger<PostamatRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<Postamat> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск постамата по ID: {id}", id);
            try
            {
                var postamat = await _db.GetTable<PostamatModel>().FirstOrDefaultAsync(p => p.PostamatId == id);
                return postamat?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении постамата по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Postamat>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех постаматов");
            try
            {
                var postamats = await _db.GetTable<PostamatModel>().ToListAsync();
                return postamats.Select(p => p.ToDomain()!).Where(p => p != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех постаматов");
                throw;
            }
        }

        public async Task<IEnumerable<Postamat>> GetActivePostamatsAsync()
        {
            _logger.LogInformation("Получение активных постаматов");
            try
            {
                var postamats = await _db.GetTable<PostamatModel>()
                    .Where(p => p.Status == "Active")
                    .ToListAsync();
                return postamats.Select(p => p.ToDomain()!).Where(p => p != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении активных постаматов");
                throw;
            }
        }

        public async Task<int> AddAsync(Postamat entity)
        {
            _logger.LogInformation("Добавление постамата по адресу: {Address}", entity?.Address);
            try
            {
                if (entity == null)
                    return 0;

                var model = entity.ToModel();
                // Возвращаем сгенерированный ID, так как это полезно при создании терминала
                return await _db.InsertWithInt32IdentityAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении постамата: {Address}", entity?.Address);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Postamat entity)
        {
            _logger.LogInformation("Обновление постамата ID: {id}", entity?.PostamatId);
            try
            {
                if (entity == null)
                    return 0;

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении постамата ID: {id}", entity?.PostamatId);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление постамата ID: {id}", id);
            try
            {
                var postamat = await _db.GetTable<PostamatModel>().FirstOrDefaultAsync(p => p.PostamatId == id);
                if (postamat is null)
                    return 0;

                return await _db.DeleteAsync(postamat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении постамата ID: {id}", id);
                throw;
            }
        }
    }
}