using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.Services
{
    public class EmployeeService : IService<EmployeeDto>
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<EmployeeService> _logger;
        private readonly AppSettings _appSettings;

        public EmployeeService(
            IEmployeeRepository repository,
            ILogger<EmployeeService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(EmployeeDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для сотрудника");
                _logger.LogDebug("Добавление нового сотрудника: {Surname} {Name}", dto.Surname, dto.Name);
            }
            _logger.LogInformation("Добавление сотрудника {Surname} {Name}", dto.Surname, dto.Name);

            try
            {
                var entity = EmployeeDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Сотрудник успешно добавлен. ID: {EmployeesId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении сотрудника {Surname} {Name}",
                    dto.Surname, dto.Name);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для сотрудника");
                _logger.LogDebug("Удаление сотрудника с ID: {EmployeesId}", id);
            }
            _logger.LogInformation("Удаление сотрудника ID: {EmployeesId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Сотрудник ID: {EmployeesId} успешно удален", id);
                }
                else
                {
                    _logger.LogWarning("Сотрудник ID: {EmployeesId} не найден", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления сотрудника ID: {EmployeesId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<EmployeeDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для сотрудников");
                _logger.LogDebug("Получение всех сотрудников");
            }
            _logger.LogInformation("Запрос всех сотрудников");

            try
            {
                var employees = await _repository.GetAllAsync();
                var result = employees.Select(EmployeeDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} сотрудников", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка сотрудников");
                throw;
            }
        }

        public async Task<EmployeeDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для сотрудника");
                _logger.LogDebug("Получение сотрудника по ID: {EmployeesId}", id);
            }
            _logger.LogInformation("Запрос сотрудника ID: {EmployeesId}", id);

            try
            {
                var employee = await _repository.GetByIdAsync(id);
                if (employee == null)
                {
                    _logger.LogWarning("Сотрудник ID: {EmployeesId} не найден", id);
                    return null;
                }

                _logger.LogInformation("Сотрудник ID: {EmployeesId} успешно получен", id);
                return EmployeeDto.ToDto(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения сотрудника ID: {EmployeesId}", id);
                throw;
            }
        }

        public async Task<bool> Update(EmployeeDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для сотрудника");
                _logger.LogDebug("Обновление сотрудника ID: {EmployeesId}", dto.EmployeesId);
            }
            _logger.LogInformation("Обновление сотрудника ID: {EmployeesId}", dto.EmployeesId);

            try
            {
                var entity = EmployeeDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Сотрудник ID: {EmployeesId} успешно обновлен", dto.EmployeesId);
                }
                else
                {
                    _logger.LogWarning("Сотрудник ID: {EmployeesId} не найден", dto.EmployeesId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления сотрудника ID: {EmployeesId}", dto.EmployeesId);
                throw;
            }
        }
    }
}