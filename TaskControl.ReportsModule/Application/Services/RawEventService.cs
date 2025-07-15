using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskControl.Core.AppSettings;
using TaskControl.Core.SharedInterfaces;
using TaskControl.ReportsModule.Application.DTOs;
using TaskControl.ReportsModule.DataAccess.Interface;

namespace TaskControl.ReportsModule.Application.Services
{
    public class RawEventService : IService<RawEventDto>
    {
        private readonly IRawEventRepository _repository;
        private readonly ILogger<RawEventService> _logger;
        private readonly AppSettings _appSettings;

        public RawEventService(
            IRawEventRepository repository,
            ILogger<RawEventService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(RawEventDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для сырого события");
                _logger.LogDebug("Добавление события: Тип={Type}, Источник={Source}", dto.Type, dto.SourceService);
            }
            _logger.LogInformation("Добавление нового сырого события типа {Type} от сервиса {Source}", dto.Type, dto.SourceService);

            try
            {
                var entity = RawEventDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Сырое событие добавлено. ID: {ReportId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления сырого события типа {Type} от сервиса {Source}",
                    dto.Type, dto.SourceService);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для сырого события");
                _logger.LogDebug("Удаление сырого события ID: {ReportId}", id);
            }
            _logger.LogInformation("Удаление сырого события ID: {ReportId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Сырое событие ID: {ReportId} удалено", id);
                }
                else
                {
                    _logger.LogWarning("Сырое событие ID: {ReportId} не найдено", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления сырого события ID: {ReportId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RawEventDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для сырых событий");
                _logger.LogDebug("Получение всех сырых событий");
            }
            _logger.LogInformation("Запрос всех сырых событий");

            try
            {
                var events = await _repository.GetAllAsync();
                var result = events.Select(RawEventDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} сырых событий", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка сырых событий");
                throw;
            }
        }

        public async Task<RawEventDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для сырого события");
                _logger.LogDebug("Получение сырого события ID: {ReportId}", id);
            }
            _logger.LogInformation("Запрос сырого события ID: {ReportId}", id);

            try
            {
                var rawEvent = await _repository.GetByIdAsync(id);
                if (rawEvent == null)
                {
                    _logger.LogWarning("Сырое событие ID: {ReportId} не найдено", id);
                    return null;
                }

                _logger.LogInformation("Сырое событие ID: {ReportId} получено", id);
                return RawEventDto.ToDto(rawEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения сырого события ID: {ReportId}", id);
                throw;
            }
        }

        public async Task<bool> Update(RawEventDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для сырого события");
                _logger.LogDebug("Обновление сырого события ID: {ReportId}", dto.ReportId);
            }
            _logger.LogInformation("Обновление сырого события ID: {ReportId}", dto.ReportId);

            try
            {
                var entity = RawEventDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Сырое событие ID: {ReportId} обновлено", dto.ReportId);
                }
                else
                {
                    _logger.LogWarning("Сырое событие ID: {ReportId} не найдено", dto.ReportId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления сырого события ID: {ReportId}", dto.ReportId);
                throw;
            }
        }
    }
}