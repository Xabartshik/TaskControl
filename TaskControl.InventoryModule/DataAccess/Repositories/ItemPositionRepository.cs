using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Mapper;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.Domain;
using TaskControl.InformationModule.Application.DTOs;

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

        public async Task<IEnumerable<AvailableItemDto>> GetAvailableItemsByBranchAsync(int branchId, string? search = null)
        {
            _logger.LogInformation("Получение доступных товаров для филиала {BranchId}. Поиск: {Search}", branchId, search);

            // 1. Подготавливаем запрос
            var query = from ip in _db.GetTable<ItemPositionModel>()
                        join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                        join i in _db.GetTable<ItemModel>() on ip.ItemId equals i.ItemId
                        where p.BranchId == branchId
                        select new { ip, p, i };

            // Применяем поиск с использованием LIKE, если передан текст
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(x => Sql.Like(x.i.Name.ToLower(), $"%{searchLower}%"));
            }
            // 2. Получаем физические остатки
            var physicalStocks = await query
                .GroupBy(x => new { x.i.ItemId, x.i.Name, x.i.Price }) // Группируем с учетом Price
                .Select(g => new
                {
                    ItemId = g.Key.ItemId,
                    Name = g.Key.Name,
                    Price = g.Key.Price,
                    TotalQuantity = g.Sum(x => x.ip.Quantity)
                }).ToListAsync();

            if (!physicalStocks.Any()) return new List<AvailableItemDto>();

            var itemIds = physicalStocks.Select(s => s.ItemId).ToList();

            // 3. Получаем текущие резервы
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

            // 4. Формируем итоговый DTO
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
                        Price = stock.Price, // Передаем цену
                        AvailableQuantity = availableQty
                    });
                }
            }

            return result.OrderBy(r => r.Name).ToList();
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
                group ip by new { i.ItemId, i.Name, i.Price } into g
                select new
                {
                    ItemId = g.Key.ItemId,
                    Name = g.Key.Name,
                    Price = g.Key.Price,
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
                        Price = stock.Price,
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

        public async Task<Dictionary<int, ItemStockDto>> GetItemBranchCountsAsync()
        {
            try
            {
                var physicalStocks = await (
                    from ip in _db.GetTable<ItemPositionModel>()
                    join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                    group ip.Quantity by new { ip.ItemId, p.BranchId } into g
                    select new
                    {
                        ItemId = g.Key.ItemId,
                        BranchId = g.Key.BranchId,
                        PhysicalQty = g.Sum()
                    }
                ).ToListAsync();

                var reservedStocks = await (
                    from res in _db.GetTable<OrderReservationModel>()
                    join ip in _db.GetTable<ItemPositionModel>() on res.ItemPositionId equals ip.Id
                    join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                    group res.Quantity by new { ip.ItemId, p.BranchId } into g
                    select new
                    {
                        ItemId = g.Key.ItemId,
                        BranchId = g.Key.BranchId,
                        ReservedQty = g.Sum()
                    }
                ).ToListAsync();

                var itemStocks = new Dictionary<int, ItemStockDto>();

                var allKeys = physicalStocks.Select(p => new { p.ItemId, p.BranchId })
                    .Union(reservedStocks.Select(r => new { r.ItemId, r.BranchId }))
                    .Distinct();

                var stockDetails = allKeys.Select(k =>
                {
                    var phys = physicalStocks.FirstOrDefault(p => p.ItemId == k.ItemId && p.BranchId == k.BranchId)?.PhysicalQty ?? 0;
                    var res = reservedStocks.FirstOrDefault(r => r.ItemId == k.ItemId && r.BranchId == k.BranchId)?.ReservedQty ?? 0;
                    var available = Math.Max(0, phys - res);
                    return new { k.ItemId, k.BranchId, Available = available };
                }).Where(x => x.Available > 0).ToList();

                var grouped = stockDetails.GroupBy(x => x.ItemId);
                foreach (var g in grouped)
                {
                    itemStocks[g.Key] = new ItemStockDto
                    {
                        BranchCount = g.Select(x => x.BranchId).Distinct().Count(),
                        TotalAvailableQuantity = g.Sum(x => x.Available)
                    };
                }

                return itemStocks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении количества филиалов и доступного количества для товаров");
                throw;
            }
        }

        public async Task<IEnumerable<BranchStockDto>> GetItemStockDistributionAsync(int itemId)
        {
            try
            {
                var physicalStocks = await (
                    from ip in _db.GetTable<ItemPositionModel>()
                    join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                    join b in _db.GetTable<BranchModel>() on p.BranchId equals b.BranchId
                    where ip.ItemId == itemId
                    group ip.Quantity by new { b.BranchId, b.BranchName, b.Address } into g
                    select new
                    {
                        BranchId = g.Key.BranchId,
                        BranchName = g.Key.BranchName,
                        Address = g.Key.Address,
                        PhysicalQty = g.Sum()
                    }
                ).ToListAsync();

                var reservedStocks = await (
                    from res in _db.GetTable<OrderReservationModel>()
                    join ip in _db.GetTable<ItemPositionModel>() on res.ItemPositionId equals ip.Id
                    join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                    where ip.ItemId == itemId
                    group res.Quantity by p.BranchId into g
                    select new
                    {
                        BranchId = g.Key,
                        ReservedQty = g.Sum()
                    }
                ).ToListAsync();

                var result = new List<BranchStockDto>();
                foreach (var phys in physicalStocks)
                {
                    var reserved = reservedStocks.FirstOrDefault(r => r.BranchId == phys.BranchId)?.ReservedQty ?? 0;
                    var available = Math.Max(0, phys.PhysicalQty - reserved);
                    if (available > 0)
                    {
                        result.Add(new BranchStockDto
                        {
                            BranchId = phys.BranchId,
                            BranchName = phys.BranchName,
                            Address = phys.Address,
                            AvailableQuantity = available
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении распределения остатков товара {ItemId}", itemId);
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
                return await _db.InsertWithInt32IdentityAsync(model);
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

        /// <summary>
        /// Проверяет доступность товаров из корзины во всех филиалах.
        /// </summary>
        public async Task<BranchAvailabilityResponseDto> CheckCartAvailabilityAsync(List<CartItemDto> cartItems)
        {
            var response = new BranchAvailabilityResponseDto();
            if (cartItems == null || !cartItems.Any())
            {
                var allBranches = await _db.GetTable<BranchModel>().ToListAsync();
                response.AvailableBranches = allBranches.Select(b => new BranchDto
                {
                    BranchId = b.BranchId,
                    BranchName = b.BranchName,
                    BranchType = b.BranchType,
                    Address = b.Address
                }).ToList();
                return response;
            }

            var itemIds = cartItems.Select(i => i.ItemId).Distinct().ToList();

            var physicalQuery = from ip in _db.GetTable<ItemPositionModel>()
                                 join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                                 where itemIds.Contains(ip.ItemId)
                                 group ip.Quantity by new { p.BranchId, ip.ItemId } into g
                                 select new
                                 {
                                     BranchId = g.Key.BranchId,
                                     ItemId = g.Key.ItemId,
                                     PhysicalQty = g.Sum()
                                 };

            var reservedQuery = from res in _db.GetTable<OrderReservationModel>()
                                 join ip in _db.GetTable<ItemPositionModel>() on res.ItemPositionId equals ip.Id
                                 join p in _db.GetTable<PositionModel>() on ip.PositionId equals p.PositionId
                                 where itemIds.Contains(ip.ItemId)
                                 group res.Quantity by new { p.BranchId, ip.ItemId } into g
                                 select new
                                 {
                                     BranchId = g.Key.BranchId,
                                     ItemId = g.Key.ItemId,
                                     ReservedQty = g.Sum()
                                 };

            var physicalStocks = await physicalQuery.ToListAsync();
            var reservedStocks = await reservedQuery.ToListAsync();
            var branches = await _db.GetTable<BranchModel>().ToListAsync();

            foreach (var b in branches)
            {
                var missingItems = new List<MissingItemDto>();
                bool isFullyAvailable = true;

                foreach (var cartItem in cartItems)
                {
                    var physQty = physicalStocks
                        .FirstOrDefault(s => s.BranchId == b.BranchId && s.ItemId == cartItem.ItemId)?.PhysicalQty ?? 0;
                    var resQty = reservedStocks
                        .FirstOrDefault(s => s.BranchId == b.BranchId && s.ItemId == cartItem.ItemId)?.ReservedQty ?? 0;

                    var availableQty = Math.Max(0, physQty - resQty);

                    if (availableQty < cartItem.RequiredQuantity)
                    {
                        isFullyAvailable = false;
                        missingItems.Add(new MissingItemDto
                        {
                            ItemId = cartItem.ItemId,
                            RequiredQuantity = cartItem.RequiredQuantity,
                            AvailableQuantity = availableQty
                        });
                    }
                }

                var branchDto = new BranchDto
                {
                    BranchId = b.BranchId,
                    BranchName = b.BranchName,
                    BranchType = b.BranchType,
                    Address = b.Address
                };

                if (isFullyAvailable)
                {
                    response.AvailableBranches.Add(branchDto);
                }
                else
                {
                    response.PartiallyAvailableBranches.Add(new BranchAvailabilityDto
                    {
                        Branch = branchDto,
                        MissingItems = missingItems
                    });
                }
            }

            return response;
        }
    }
}