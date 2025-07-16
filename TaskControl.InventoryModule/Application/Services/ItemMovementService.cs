using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для перемещения товара");
                _logger.LogDebug("Добавление перемещения: Товары={Quantity}", dto.Quantity);
            }
            _logger.LogInformation("Добавление перемещения {Quantity} товаров", dto.Quantity);

            try
            {
                var entity = ItemMovementDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Перемещение добавлено. ID: {MovementId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления перемещения {Quantity} товаров", dto.Quantity);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для перемещения");
                _logger.LogDebug("Удаление перемещения ID: {MovementId}", id);
            }
            _logger.LogInformation("Удаление перемещения ID: {MovementId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Перемещение ID: {MovementId} удалено", id);
                }
                else
                {
                    _logger.LogWarning("Перемещение ID: {MovementId} не найдено", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления перемещения ID: {MovementId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ItemMovementDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для перемещений");
                _logger.LogDebug("Получение всех перемещений");
            }
            _logger.LogInformation("Запрос всех перемещений товаров");

            try
            {
                var movements = await _repository.GetAllAsync();
                var result = movements.Select(ItemMovementDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} перемещений", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка перемещений");
                throw;
            }
        }

        public async Task<ItemMovementDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для перемещения");
                _logger.LogDebug("Получение перемещения ID: {MovementId}", id);
            }
            _logger.LogInformation("Запрос перемещения ID: {MovementId}", id);

            try
            {
                var movement = await _repository.GetByIdAsync(id);
                if (movement == null)
                {
                    _logger.LogWarning("Перемещение ID: {MovementId} не найдено", id);
                    return null;
                }

                _logger.LogInformation("Перемещение ID: {MovementId} получено", id);
                return ItemMovementDto.ToDto(movement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения перемещения ID: {MovementId}", id);
                throw;
            }
        }

        public async Task<bool> Update(ItemMovementDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для перемещения");
                _logger.LogDebug("Обновление перемещения ID: {MovementId}", dto.Id);
            }
            _logger.LogInformation("Обновление перемещения ID: {MovementId}", dto.Id);

            try
            {
                var entity = ItemMovementDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Перемещение ID: {MovementId} обновлено", dto.Id);
                }
                else
                {
                    _logger.LogWarning("Перемещение ID: {MovementId} не найдено", dto.Id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления перемещения ID: {MovementId}", dto.Id);
                throw;
            }
        }
    }
}