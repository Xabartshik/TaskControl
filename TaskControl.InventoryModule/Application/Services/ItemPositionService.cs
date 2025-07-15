using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.InventoryModule.DataAccess.Interface;

namespace TaskControl.OrderModule.Application.Services
{
    public class ItemPositionService : IService<ItemPositionDto>
    {
        private readonly IItemPositionRepository _repository;
        private readonly ILogger<ItemPositionService> _logger;
        private readonly AppSettings _appSettings;

        public ItemPositionService(
            IItemPositionRepository repository,
            ILogger<ItemPositionService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(ItemPositionDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для товарной позиции");
                _logger.LogDebug("Добавление позиции: Товар={ItemId}, Позиция={PositionId}",
                    dto.ItemId, dto.PositionId);
            }
            _logger.LogInformation("Добавление товара {ItemId} в позицию {PositionId}",
                dto.ItemId, dto.PositionId);

            try
            {
                var entity = ItemPositionDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Товарная позиция добавлена. ID: {PositionId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления товара {ItemId} в позицию {PositionId}",
                    dto.ItemId, dto.PositionId);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для товарной позиции");
                _logger.LogDebug("Удаление позиции ID: {PositionId}", id);
            }
            _logger.LogInformation("Удаление товарной позиции ID: {PositionId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Товарная позиция ID: {PositionId} удалена", id);
                }
                else
                {
                    _logger.LogWarning("Товарная позиция ID: {PositionId} не найдена", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления товарной позиции ID: {PositionId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ItemPositionDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для товарных позиций");
                _logger.LogDebug("Получение всех товарных позиций");
            }
            _logger.LogInformation("Запрос всех товарных позиций");

            try
            {
                var positions = await _repository.GetAllAsync();
                var result = positions.Select(ItemPositionDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} товарных позиций", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка товарных позиций");
                throw;
            }
        }

        public async Task<ItemPositionDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для товарной позиции");
                _logger.LogDebug("Получение позиции ID: {PositionId}", id);
            }
            _logger.LogInformation("Запрос товарной позиции ID: {PositionId}", id);

            try
            {
                var position = await _repository.GetByIdAsync(id);
                if (position == null)
                {
                    _logger.LogWarning("Товарная позиция ID: {PositionId} не найдена", id);
                    return null;
                }

                _logger.LogInformation("Товарная позиция ID: {PositionId} получена", id);
                return ItemPositionDto.ToDto(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения товарной позиции ID: {PositionId}", id);
                throw;
            }
        }

        public async Task<bool> Update(ItemPositionDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для товарной позиции");
                _logger.LogDebug("Обновление позиции ID: {PositionId}", dto.Id);
            }
            _logger.LogInformation("Обновление товарной позиции ID: {PositionId}", dto.Id);

            try
            {
                var entity = ItemPositionDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Товарная позиция ID: {PositionId} обновлена", dto.Id);
                }
                else
                {
                    _logger.LogWarning("Товарная позиция ID: {PositionId} не найдена", dto.Id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления товарной позиции ID: {PositionId}", dto.Id);
                throw;
            }
        }
    }
}