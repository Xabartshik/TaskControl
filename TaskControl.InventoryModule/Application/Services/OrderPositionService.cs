using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.OrderModule.Application.DTOs;

namespace TaskControl.OrderModule.Services
{
    public class OrderPositionService : IService<OrderPositionDto>
    {
        private readonly IOrderPositionRepository _repository;
        private readonly ILogger<OrderPositionService> _logger;
        private readonly AppSettings _appSettings;

        public OrderPositionService(
            IOrderPositionRepository repository,
            ILogger<OrderPositionService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(OrderPositionDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для позиции заказа");
                _logger.LogDebug("Добавление позиции: Заказ={OrderId}, Позиция={PositionId}",
                    dto.OrderId, dto.ItemPositionId);
            }
            _logger.LogInformation("Добавление позиции {PositionId} в заказ {OrderId}",
                dto.ItemPositionId, dto.OrderId);

            try
            {
                var entity = OrderPositionDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Позиция заказа добавлена. ID: {PositionId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления позиции {PositionId} в заказ {OrderId}",
                    dto.ItemPositionId, dto.OrderId);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для позиции заказа");
                _logger.LogDebug("Удаление позиции заказа ID: {PositionId}", id);
            }
            _logger.LogInformation("Удаление позиции заказа ID: {PositionId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Позиция заказа ID: {PositionId} удалена", id);
                }
                else
                {
                    _logger.LogWarning("Позиция заказа ID: {PositionId} не найдена", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления позиции заказа ID: {PositionId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<OrderPositionDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для позиций заказов");
                _logger.LogDebug("Получение всех позиций заказов");
            }
            _logger.LogInformation("Запрос всех позиций заказов");

            try
            {
                var positions = await _repository.GetAllAsync();
                var result = positions.Select(OrderPositionDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} позиций заказов", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка позиций заказов");
                throw;
            }
        }

        public async Task<OrderPositionDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для позиции заказа");
                _logger.LogDebug("Получение позиции заказа ID: {PositionId}", id);
            }
            _logger.LogInformation("Запрос позиции заказа ID: {PositionId}", id);

            try
            {
                var position = await _repository.GetByIdAsync(id);
                if (position == null)
                {
                    _logger.LogWarning("Позиция заказа ID: {PositionId} не найдена", id);
                    return null;
                }

                _logger.LogInformation("Позиция заказа ID: {PositionId} получена", id);
                return OrderPositionDto.ToDto(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения позиции заказа ID: {PositionId}", id);
                throw;
            }
        }

        public async Task<bool> Update(OrderPositionDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для позиции заказа");
                _logger.LogDebug("Обновление позиции заказа ID: {PositionId}", dto.UniqueId);
            }
            _logger.LogInformation("Обновление позиции заказа ID: {PositionId}", dto.UniqueId);

            try
            {
                var entity = OrderPositionDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Позиция заказа ID: {PositionId} обновлена", dto.UniqueId);
                }
                else
                {
                    _logger.LogWarning("Позиция заказа ID: {PositionId} не найдена", dto.UniqueId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления позиции заказа ID: {PositionId}", dto.UniqueId);
                throw;
            }
        }
    }
}
