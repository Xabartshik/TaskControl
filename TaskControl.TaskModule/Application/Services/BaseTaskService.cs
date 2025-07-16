using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.DataAccess.Interface;

namespace TaskControl.TaskModule.Application.Services
{
    public class BaseTaskService : IService<BaseTaskDto>
    {
        private readonly IActiveTaskRepository _repository;
        private readonly ILogger<BaseTaskService> _logger;
        private readonly AppSettings _appSettings;

        public BaseTaskService(
            IActiveTaskRepository repository,
            ILogger<BaseTaskService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(BaseTaskDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для активной задачи");
                _logger.LogDebug("Добавление задачи: Тип={Type}, Филиал={BranchId}", dto.Type, dto.BranchId);
            }
            _logger.LogInformation("Добавление новой активной задачи типа {Type} для филиала {BranchId}",
                dto.Type, dto.BranchId);

            try
            {
                var entity = BaseTaskDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Активная задача добавлена. ID: {TaskId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления активной задачи типа {Type} для филиала {BranchId}",
                    dto.Type, dto.BranchId);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для активной задачи");
                _logger.LogDebug("Удаление задачи ID: {TaskId}", id);
            }
            _logger.LogInformation("Удаление активной задачи ID: {TaskId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Активная задача ID: {TaskId} удалена", id);
                }
                else
                {
                    _logger.LogWarning("Активная задача ID: {TaskId} не найдена", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления активной задачи ID: {TaskId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<BaseTaskDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для активных задач");
                _logger.LogDebug("Получение всех активных задач");
            }
            _logger.LogInformation("Запрос всех активных задач");

            try
            {
                var tasks = await _repository.GetAllAsync();
                var result = tasks.Select(BaseTaskDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} активных задач", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка активных задач");
                throw;
            }
        }


        public async Task<BaseTaskDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для активной задачи");
                _logger.LogDebug("Получение задачи ID: {TaskId}", id);
            }
            _logger.LogInformation("Запрос активной задачи ID: {TaskId}", id);

            try
            {
                var task = await _repository.GetByIdAsync(id);
                if (task == null)
                {
                    _logger.LogWarning("Активная задача ID: {TaskId} не найдена", id);
                    return null;
                }

                _logger.LogInformation("Активная задача ID: {TaskId} получена", id);
                return BaseTaskDto.ToDto(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения активной задачи ID: {TaskId}", id);
                throw;
            }
        }

        public async Task<bool> Update(BaseTaskDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для активной задачи");
                _logger.LogDebug("Обновление задачи ID: {TaskId}", dto.TaskId);
            }
            _logger.LogInformation("Обновление активной задачи ID: {TaskId}", dto.TaskId);

            try
            {
                var entity = BaseTaskDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Активная задача ID: {TaskId} обновлена", dto.TaskId);
                }
                else
                {
                    _logger.LogWarning("Активная задача ID: {TaskId} не найдена", dto.TaskId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления активной задачи ID: {TaskId}", dto.TaskId);
                throw;
            }
        }
    }
}