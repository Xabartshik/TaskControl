using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.Domain;
using UnitsNet;

namespace TaskControl.InformationModule.Services
{
    public class ItemService : IService<ItemDto>
    {
        private readonly IItemRepository _repository;
        private readonly ILogger<ItemService> _logger;
        private readonly AppSettings _appSettings;

        public ItemService(
            IItemRepository repository,
            ILogger<ItemService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(ItemDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для товара");
                _logger.LogDebug("Добавление нового товара");
            }
            _logger.LogInformation("Добавление товара ID: {ItemId}", dto.ItemId);

            try
            {
                var entity = ItemDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Товар успешно добавлен. ID: {ItemId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении товара ID: {ItemId}", dto.ItemId);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для товара");
                _logger.LogDebug("Удаление товара ID: {ItemId}", id);
            }
            _logger.LogInformation("Удаление товара ID: {ItemId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Товар ID: {ItemId} успешно удален", id);
                }
                else
                {
                    _logger.LogWarning("Товар ID: {ItemId} не найден", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления товара ID: {ItemId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ItemDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для товаров");
                _logger.LogDebug("Получение всех товаров");
            }
            _logger.LogInformation("Запрос всех товаров");

            try
            {
                var items = await _repository.GetAllAsync();
                var result = items.Select(ItemDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} товаров", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка товаров");
                throw;
            }
        }

        public async Task<ItemDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для товара");
                _logger.LogDebug("Получение товара по ID: {ItemId}", id);
            }
            _logger.LogInformation("Запрос товара ID: {ItemId}", id);

            try
            {
                var item = await _repository.GetByIdAsync(id);
                if (item == null)
                {
                    _logger.LogWarning("Товар ID: {ItemId} не найден", id);
                    return null;
                }

                _logger.LogInformation("Товар ID: {ItemId} успешно получен", id);
                return ItemDto.ToDto(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения товара ID: {ItemId}", id);
                throw;
            }
        }

        public async Task<bool> Update(ItemDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для товара");
                _logger.LogDebug("Обновление товара ID: {ItemId}", dto.ItemId);
            }
            _logger.LogInformation("Обновление товара ID: {ItemId}", dto.ItemId);

            try
            {
                var entity = ItemDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Товар ID: {ItemId} успешно обновлен", dto.ItemId);
                }
                else
                {
                    _logger.LogWarning("Товар ID: {ItemId} не найден", dto.ItemId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления товара ID: {ItemId}", dto.ItemId);
                throw;
            }
        }
    }
}