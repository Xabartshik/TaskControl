using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.Services
{
    public class CheckIOEmployeeService : IService<CheckIOEmployeeDto>
    {
        private readonly ICheckIOEmployeeRepository _repository;
        private readonly ILogger<CheckIOEmployeeService> _logger;
        private readonly AppSettings _appSettings;

        public CheckIOEmployeeService(
            ICheckIOEmployeeRepository repository,
            ILogger<CheckIOEmployeeService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(CheckIOEmployeeDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для отметки сотрудника");
                _logger.LogDebug("Добавление отметки: Сотрудник={EmployeeId}, Тип={CheckType}",
                    dto.EmployeeId, dto.CheckType);
            }
            _logger.LogInformation("Добавление отметки для сотрудника {EmployeeId}", dto.EmployeeId);

            try
            {
                var entity = CheckIOEmployeeDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Отметка добавлена. ID: {Id}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления отметки для сотрудника {EmployeeId}", dto.EmployeeId);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для отметки");
                _logger.LogDebug("Удаление отметки ID: {Id}", id);
            }
            _logger.LogInformation("Удаление отметки ID: {Id}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Отметка ID: {Id} удалена", id);
                }
                else
                {
                    _logger.LogWarning("Отметка ID: {Id} не найдена", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления отметки ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<CheckIOEmployeeDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для отметок");
                _logger.LogDebug("Получение всех отметок");
            }
            _logger.LogInformation("Запрос всех отметок");

            try
            {
                var records = await _repository.GetAllAsync();
                var result = records.Select(CheckIOEmployeeDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} отметок", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка отметок");
                throw;
            }
        }

        public async Task<CheckIOEmployeeDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для отметки");
                _logger.LogDebug("Получение отметки ID: {Id}", id);
            }
            _logger.LogInformation("Запрос отметки ID: {Id}", id);

            try
            {
                var record = await _repository.GetByIdAsync(id);
                if (record == null)
                {
                    _logger.LogWarning("Отметка ID: {Id} не найдена", id);
                    return null;
                }

                _logger.LogInformation("Отметка ID: {Id} получена", id);
                return CheckIOEmployeeDto.ToDto(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения отметки ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> Update(CheckIOEmployeeDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для отметки");
                _logger.LogDebug("Обновление отметки ID: {Id}", dto.Id);
            }
            _logger.LogInformation("Обновление отметки ID: {Id}", dto.Id);

            try
            {
                var entity = CheckIOEmployeeDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Отметка ID: {Id} обновлена", dto.Id);
                }
                else
                {
                    _logger.LogWarning("Отметка ID: {Id} не найдена", dto.Id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления отметки ID: {Id}", dto.Id);
                throw;
            }
        }
    }
}