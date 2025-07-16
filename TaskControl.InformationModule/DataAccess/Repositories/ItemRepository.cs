using LinqToDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Mapper;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DAL.Repositories
{
    public class ItemRepository : IRepository<Item>, IItemRepository
    {
        private readonly IInformationDataConnection _db;
        private readonly ILogger<ItemRepository> _logger;

        public ItemRepository(IInformationDataConnection db, ILogger<ItemRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<Item?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск товара по ID: {id}", id);
            try
            {
                var item = await _db.Items.FirstOrDefaultAsync(i => i.ItemId == id);
                return item?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении товара по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех товаров");
            try
            {
                var itemsModel = await _db.Items.ToListAsync();
                return itemsModel.Select(i => i.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка товаров");
                throw;
            }
        }

        public async Task<int> AddAsync(Item entity)
        {
            _logger.LogInformation("Добавление нового товара ID: {id}", entity.ItemId);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var model = entity.ToModel();
                return await _db.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении товара ID: {id}", entity?.ItemId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Item entity)
        {
            _logger.LogInformation("Обновление товара ID: {id}", entity.ItemId);
            try
            {
                if (entity == null)
                    return 0;

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении товара ID: {id}", entity?.ItemId);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление товара ID: {id}", id);
            try
            {
                var item = await _db.Items.FirstOrDefaultAsync(i => i.ItemId == id);
                if (item is null)
                    return 0;

                return await _db.DeleteAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении товара ID: {id}", id);
                throw;
            }
        }
    }
}