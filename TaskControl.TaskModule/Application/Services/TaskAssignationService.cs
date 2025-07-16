using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.DataAccess.Interface;

namespace TaskControl.TaskModule.Application.Services
{
    public class TaskAssignationService : IService<TaskAssignationDto>
    {
        private readonly ITaskAssignationRepository _repository;
        private readonly ILogger<TaskAssignationService> _logger;
        private readonly AppSettings _appSettings;

        public TaskAssignationService(
            ITaskAssignationRepository repository,
            ILogger<TaskAssignationService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(TaskAssignationDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для назначения задачи");
                _logger.LogDebug("Добавление назначения: Задача={TaskId}, Пользователь={UserId}",
                    dto.TaskId, dto.UserId);
            }
            _logger.LogInformation("Назначение задачи {TaskId} на пользователя {UserId}",
                dto.TaskId, dto.UserId);

            try
            {
                var entity = TaskAssignationDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Назначение добавлено. ID: {AssignationId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка назначения задачи {TaskId} на пользователя {UserId}",
                    dto.TaskId, dto.UserId);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для назначения");
                _logger.LogDebug("Удаление назначения ID: {AssignationId}", id);
            }
            _logger.LogInformation("Удаление назначения ID: {AssignationId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Назначение ID: {AssignationId} удалено", id);
                }
                else
                {
                    _logger.LogWarning("Назначение ID: {AssignationId} не найдено", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления назначения ID: {AssignationId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<TaskAssignationDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для назначений");
                _logger.LogDebug("Получение всех назначений задач");
            }
            _logger.LogInformation("Запрос всех назначений задач");

            try
            {
                var assignations = await _repository.GetAllAsync();
                var result = assignations.Select(TaskAssignationDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} назначений задач", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка назначений задач");
                throw;
            }
        }

        public async Task<TaskAssignationDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для назначения");
                _logger.LogDebug("Получение назначения ID: {AssignationId}", id);
            }
            _logger.LogInformation("Запрос назначения ID: {AssignationId}", id);

            try
            {
                var assignation = await _repository.GetByIdAsync(id);
                if (assignation == null)
                {
                    _logger.LogWarning("Назначение ID: {AssignationId} не найдено", id);
                    return null;
                }

                _logger.LogInformation("Назначение ID: {AssignationId} получено", id);
                return TaskAssignationDto.ToDto(assignation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения назначения ID: {AssignationId}", id);
                throw;
            }
        }

        public async Task<bool> Update(TaskAssignationDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для назначения");
                _logger.LogDebug("Обновление назначения ID: {AssignationId}", dto.Id);
            }
            _logger.LogInformation("Обновление назначения ID: {AssignationId}", dto.Id);

            try
            {
                var entity = TaskAssignationDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Назначение ID: {AssignationId} обновлено", dto.Id);
                }
                else
                {
                    _logger.LogWarning("Назначение ID: {AssignationId} не найдено", dto.Id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления назначения ID: {AssignationId}", dto.Id);
                throw;
            }
        }

        public async Task<IEnumerable<TaskAssignationDto>> GetByTaskId(int taskId)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetByTaskId");
                _logger.LogDebug("Получение назначений для задачи ID: {TaskId}", taskId);
            }
            _logger.LogInformation("Запрос назначений для задачи ID: {TaskId}", taskId);

            try
            {
                var assignations = await _repository.GetByTaskIdAsync(taskId);
                var result = assignations.Select(TaskAssignationDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} назначений для задачи ID: {TaskId}",
                    result.Count, taskId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения назначений для задачи ID: {TaskId}", taskId);
                throw;
            }
        }

        public async Task<IEnumerable<TaskAssignationDto>> GetByUserId(int userId)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetByUserId");
                _logger.LogDebug("Получение назначений для пользователя ID: {UserId}", userId);
            }
            _logger.LogInformation("Запрос назначений для пользователя ID: {UserId}", userId);

            try
            {
                var assignations = await _repository.GetByUserIdAsync(userId);
                var result = assignations.Select(TaskAssignationDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} назначений для пользователя ID: {UserId}",
                    result.Count, userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения назначений для пользователя ID: {UserId}", userId);
                throw;
            }
        }
    }
}