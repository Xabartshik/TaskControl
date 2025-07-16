using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.Domain;
using UnitsNet;

namespace TaskControl.InventoryModule.Application.Services
{
    public class PositionCellService : IService<PositionCellDto>
    {
        private readonly IPositionCellRepository _repository;
        private readonly ILogger<PositionCellService> _logger;
        private readonly AppSettings _appSettings;

        public PositionCellService(
            IPositionCellRepository repository,
            ILogger<PositionCellService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(PositionCellDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для складской позиции");
                _logger.LogDebug("Добавление новой позиции: Зона={Zone}, Тип={StorageType}",
                    dto.ZoneCode, dto.FirstLevelStorageType);
            }
            _logger.LogInformation("Добавление складской позиции в зоне '{Zone}'", dto.ZoneCode);

            try
            {
                var entity = PositionCellDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Складская позиция добавлена. ID: {PositionId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления складской позиции в зоне '{Zone}'", dto.ZoneCode);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для складской позиции");
                _logger.LogDebug("Удаление позиции ID: {PositionId}", id);
            }
            _logger.LogInformation("Удаление складской позиции ID: {PositionId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Позиция ID: {PositionId} удалена", id);
                }
                else
                {
                    _logger.LogWarning("Позиция ID: {PositionId} не найдена", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления позиции ID: {PositionId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PositionCellDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для складских позиций");
                _logger.LogDebug("Получение всех позиций");
            }
            _logger.LogInformation("Запрос всех складских позиций");

            try
            {
                var positions = await _repository.GetAllAsync();
                var result = positions.Select(PositionCellDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} позиций", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка позиций");
                throw;
            }
        }

        public async Task<PositionCellDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для складской позиции");
                _logger.LogDebug("Получение позиции ID: {PositionId}", id);
            }
            _logger.LogInformation("Запрос позиции ID: {PositionId}", id);

            try
            {
                var position = await _repository.GetByIdAsync(id);
                if (position == null)
                {
                    _logger.LogWarning("Позиция ID: {PositionId} не найдена", id);
                    return null;
                }

                _logger.LogInformation("Позиция ID: {PositionId} получена", id);
                return PositionCellDto.ToDto(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения позиции ID: {PositionId}", id);
                throw;
            }
        }

        public async Task<bool> Update(PositionCellDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для складской позиции");
                _logger.LogDebug("Обновление позиции ID: {PositionId}", dto.PositionId);
            }
            _logger.LogInformation("Обновление позиции ID: {PositionId}", dto.PositionId);

            try
            {
                var entity = PositionCellDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Позиция ID: {PositionId} обновлена", dto.PositionId);
                }
                else
                {
                    _logger.LogWarning("Позиция ID: {PositionId} не найдена", dto.PositionId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления позиции ID: {PositionId}", dto.PositionId);
                throw;
            }
        }
    }

}