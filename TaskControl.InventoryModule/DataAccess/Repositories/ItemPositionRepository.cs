using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Mapper;
using TaskControl.InventoryModule.DataAccess.Model;
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

        public async Task<IEnumerable<ItemPosition>> GetByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));

            // Чтобы не гонять запрос с пустым IN()
            var idArray = ids.Distinct().ToArray();
            if (idArray.Length == 0)
                return Array.Empty<ItemPosition>();

            _logger.LogInformation("Получение связей товар-позиция по списку ID. Count: {count}", idArray.Length);

            try
            {
                var models = await _db.ItemPositions
                    .Where(ip => idArray.Contains(ip.Id))
                    .ToListAsync();

                return models.Select(m => m.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении связей товар-позиция по списку ID");
                throw;
            }
        }

        public async Task<IEnumerable<AvailableItemDto>> GetAvailableItemsByBranchAsync(int branchId)
        {
            _logger.LogInformation("Получение доступных товаров для филиала {BranchId}", branchId);

            // 1. Получаем физические остатки
            var physicalStocks = await (
                from ip in _db.GetTable<ItemPositionModel>()
                join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                join i in _db.GetTable<ItemModel>() on ip.ItemId equals i.ItemId
                where p.BranchId == branchId
                group ip by new { i.ItemId, i.Name } into g
                select new
                {
                    ItemId = g.Key.ItemId,
                    Name = g.Key.Name,
                    TotalQuantity = g.Sum(x => x.Quantity)
                }
            ).ToListAsync();

            if (!physicalStocks.Any()) return new List<AvailableItemDto>();

            var itemIds = physicalStocks.Select(s => s.ItemId).ToList();

            // 2. Получаем текущие резервы (Hard Allocation)
            var reservedStocks = await (
                from res in _db.GetTable<OrderReservationModel>()
                join ip in _db.GetTable<ItemPositionModel>() on res.ItemPositionId equals ip.Id
                join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                where p.BranchId == branchId && itemIds.Contains(ip.ItemId)
                group res by ip.ItemId into g
                select new
                {
                    ItemId = g.Key,
                    ReservedQuantity = g.Sum(x => x.Quantity)
                }
            ).ToListAsync();

            // 3. Формируем итоговый DTO
            var result = new List<AvailableItemDto>();
            foreach (var stock in physicalStocks)
            {
                var reserved = reservedStocks.FirstOrDefault(r => r.ItemId == stock.ItemId)?.ReservedQuantity ?? 0;
                var availableQty = stock.TotalQuantity - reserved;

                if (availableQty > 0)
                {
                    result.Add(new AvailableItemDto
                    {
                        ItemId = stock.ItemId,
                        Name = stock.Name,
                        AvailableQuantity = availableQty
                    });
                }
            }

            return result.OrderBy(r => r.Name).ToList();
        }

        public async Task<IEnumerable<ItemPosition>> GetByItemAndBranchAsync(int branchId, int itemId)
        {
            _logger.LogInformation("Поиск позиций товара {ItemId} в филиале {BranchId} через БД", itemId, branchId);
            try
            {
                // Выполняем JOIN на уровне базы данных
                var query = from ip in _db.ItemPositions
                            join p in _db.PositionCells on ip.PositionId equals p.PositionId
                            where ip.ItemId == itemId && p.BranchId == branchId
                            select ip;

                var results = await query.ToListAsync();
                return results.Select(ip => ip.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка БД при поиске товара {ItemId} в филиале {BranchId}", itemId, branchId);
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