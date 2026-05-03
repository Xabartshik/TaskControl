using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.InventoryModule.DataAccess.Interface;

namespace TaskControl.InventoryModule.Application.Services
{
    public class ItemMovementService : IService<ItemMovementDto>
    {
        private readonly IItemMovementRepository _repository;
        private readonly ILogger<ItemMovementService> _logger;
        private readonly AppSettings _appSettings;

        public ItemMovementService(
            IItemMovementRepository repository,
            ILogger<ItemMovementService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(ItemMovementDto dto)
        {
            _logger.LogInformation("Добавление перемещения товара {ItemId}, кол-во: {Quantity}", dto.ItemId, dto.Quantity);
            try
            {
                var entity = ItemMovementDto.FromDto(dto);
                return await _repository.AddAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления перемещения");
                throw;
            }
        }

        public async Task<IEnumerable<ItemMovementDto>> GetAll()
        {
            var movements = await _repository.GetAllAsync();
            return movements.Select(ItemMovementDto.ToDto);
        }

        public async Task<ItemMovementDto?> GetById(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : ItemMovementDto.ToDto(entity);
        }

        public async Task<bool> Update(ItemMovementDto dto)
        {
            var entity = ItemMovementDto.FromDto(dto);
            return await _repository.UpdateAsync(entity) > 0;
        }

        public async Task<bool> Delete(int id)
        {
            return await _repository.DeleteAsync(id) > 0;
        }
    }
}