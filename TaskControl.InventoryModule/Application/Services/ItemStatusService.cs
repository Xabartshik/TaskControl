using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.InventoryModule.DataAccess.Interface;

namespace TaskControl.OrderModule.Application.Services
{
    public class ItemStatusService : IService<ItemStatusDto>
    {
        private readonly IItemStatusRepository _repository;
        private readonly ILogger<ItemStatusService> _logger;
        private readonly AppSettings _appSettings;

        public ItemStatusService(
            IItemStatusRepository repository,
            ILogger<ItemStatusService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(ItemStatusDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для статуса товара");
                _logger.LogDebug("Добавление статуса: Позиция={PositionId}, Статус={Status}",
                    dto.ItemPositionId, dto.Status);
            }
            _logger.LogInformation("Добавление статуса '{Status}' для позиции {PositionId}",
                dto.Status, dto.ItemPositionId);

            try
            {
                var entity = ItemStatusDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Статус добавлен. ID: {StatusId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления статуса '{Status}' для позиции {PositionId}",
                    dto.Status, dto.ItemPositionId);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для статуса товара");
                _logger.LogDebug("Удаление статуса ID: {StatusId}", id);
            }
            _logger.LogInformation("Удаление статуса товара ID: {StatusId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Статус ID: {StatusId} удален", id);
                }
                else
                {
                    _logger.LogWarning("Статус ID: {StatusId} не найден", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления статуса ID: {StatusId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ItemStatusDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для статусов товаров");
                _logger.LogDebug("Получение всех статусов");
            }
            _logger.LogInformation("Запрос всех статусов товаров");

            try
            {
                var statuses = await _repository.GetAllAsync();
                var result = statuses.Select(ItemStatusDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} статусов", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка статусов");
                throw;
            }
        }

        public async Task<ItemStatusDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для статуса товара");
                _logger.LogDebug("Получение статуса ID: {StatusId}", id);
            }
            _logger.LogInformation("Запрос статуса товара ID: {StatusId}", id);

            try
            {
                var status = await _repository.GetByIdAsync(id);
                if (status == null)
                {
                    _logger.LogWarning("Статус ID: {StatusId} не найден", id);
                    return null;
                }

                _logger.LogInformation("Статус ID: {StatusId} получен", id);
                return ItemStatusDto.ToDto(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения статуса ID: {StatusId}", id);
                throw;
            }
        }

        public async Task<bool> Update(ItemStatusDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для статуса товара");
                _logger.LogDebug("Обновление статуса ID: {StatusId}", dto.Id);
            }
            _logger.LogInformation("Обновление статуса товара ID: {StatusId}", dto.Id);

            try
            {
                var entity = ItemStatusDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Статус ID: {StatusId} обновлен", dto.Id);
                }
                else
                {
                    _logger.LogWarning("Статус ID: {StatusId} не найден", dto.Id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления статуса ID: {StatusId}", dto.Id);
                throw;
            }
        }
    }
}