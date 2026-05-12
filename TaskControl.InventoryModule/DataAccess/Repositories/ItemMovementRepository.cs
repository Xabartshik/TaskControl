using LinqToDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Mapper;
using TaskControl.InventoryModule.DataAccess.Model;
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
            _logger.LogInformation("Поиск перемещения по ID: {id}", id);
            var model = await _db.ItemMovements.FirstOrDefaultAsync(m => m.Id == id);
            return model?.ToDomain();
        }

        public async Task<IEnumerable<ItemMovement>> GetAllAsync()
        {
            var models = await _db.ItemMovements.ToListAsync();
            return models.Select(m => m.ToDomain());
        }

        public async Task<int> AddAsync(ItemMovement entity)
        {
            if (entity.Quantity <= 0) throw new ArgumentException("Количество должно быть > 0");
            var model = entity.ToModel();
            return await _db.InsertAsync(model);
        }

        public async Task<int> UpdateAsync(ItemMovement entity)
        {
            var model = entity.ToModel();
            return await _db.UpdateAsync(model);
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _db.ItemMovements.Where(m => m.Id == id).DeleteAsync();
        }

        public async Task<IEnumerable<ItemMovement>> GetBySourcePositionAsync(int positionId)
        {
            var models = await _db.ItemMovements.Where(m => m.SourcePositionId == positionId).ToListAsync();
            return models.Select(m => m.ToDomain());
        }

        public async Task<IEnumerable<ItemMovement>> GetByDestinationPositionAsync(int positionId)
        {
            var models = await _db.ItemMovements.Where(m => m.DestinationPositionId == positionId).ToListAsync();
            return models.Select(m => m.ToDomain());
        }

        public async Task<IEnumerable<ItemMovement>> GetByBranchAsync(int branchId)
        {
            var models = await _db.ItemMovements
                .Where(m => m.SourceBranchId == branchId || m.DestinationBranchId == branchId)
                .ToListAsync();
            return models.Select(m => m.ToDomain());
        }

        public async Task<IEnumerable<ItemMovement>> GetByItemAsync(int itemId)
        {
            var models = await _db.ItemMovements.Where(m => m.ItemId == itemId).ToListAsync();
            return models.Select(m => m.ToDomain());
        }
    }
}