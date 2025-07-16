using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.OrderModule.Application.DTOs;
using TaskControl.OrderModule.DataAccess.Interface;

namespace TaskControl.OrderModule.Application.Services
{
    public class OrderService : IService<OrderDto>
    {
        private readonly IOrderRepository _repository;
        private readonly ILogger<OrderService> _logger;
        private readonly AppSettings _appSettings;

        public OrderService(
            IOrderRepository repository,
            ILogger<OrderService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(OrderDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для заказа");
                _logger.LogDebug("Добавление заказа: Клиент={CustomerId}, Тип={Type}", dto.CustomerId, dto.Type);
            }
            _logger.LogInformation("Добавление нового заказа типа {Type} для клиента {CustomerId}", dto.Type, dto.CustomerId);

            try
            {
                var entity = OrderDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Заказ добавлен. ID: {OrderId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления заказа типа {Type} для клиента {CustomerId}", dto.Type, dto.CustomerId);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для заказа");
                _logger.LogDebug("Удаление заказа ID: {OrderId}", id);
            }
            _logger.LogInformation("Удаление заказа ID: {OrderId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Заказ ID: {OrderId} удален", id);
                }
                else
                {
                    _logger.LogWarning("Заказ ID: {OrderId} не найден", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления заказа ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для заказов");
                _logger.LogDebug("Получение всех заказов");
            }
            _logger.LogInformation("Запрос всех заказов");

            try
            {
                var orders = await _repository.GetAllAsync();
                var result = orders.Select(OrderDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} заказов", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка заказов");
                throw;
            }
        }

        public async Task<OrderDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для заказа");
                _logger.LogDebug("Получение заказа ID: {OrderId}", id);
            }
            _logger.LogInformation("Запрос заказа ID: {OrderId}", id);

            try
            {
                var order = await _repository.GetByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Заказ ID: {OrderId} не найден", id);
                    return null;
                }

                _logger.LogInformation("Заказ ID: {OrderId} получен", id);
                return OrderDto.ToDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения заказа ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> Update(OrderDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для заказа");
                _logger.LogDebug("Обновление заказа ID: {OrderId}", dto.OrderId);
            }
            _logger.LogInformation("Обновление заказа ID: {OrderId}", dto.OrderId);

            try
            {
                var entity = OrderDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Заказ ID: {OrderId} обновлен", dto.OrderId);
                }
                else
                {
                    _logger.LogWarning("Заказ ID: {OrderId} не найден", dto.OrderId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления заказа ID: {OrderId}", dto.OrderId);
                throw;
            }
        }
    }
}