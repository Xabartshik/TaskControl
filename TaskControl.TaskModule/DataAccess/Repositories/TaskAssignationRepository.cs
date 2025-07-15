using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.SharedInterfaces;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DAL.Repositories
{
    public class TaskAssignationRepository : IRepository<TaskAssignation>
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<TaskAssignationRepository> _logger;

        public TaskAssignationRepository(ITaskDataConnection db, ILogger<TaskAssignationRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<TaskAssignation?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск назначения задачи по ID: {id}", id);
            try
            {
                var assignation = await _db.TaskAssignations.FirstOrDefaultAsync(a => a.Id == id);
                return assignation?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении назначения задачи по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<TaskAssignation>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех назначений задач");
            try
            {
                var assignations = await _db.TaskAssignations.ToListAsync();
                return assignations.Select(a => a.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка назначений задач");
                throw;
            }
        }

        public async Task<int> AddAsync(TaskAssignation entity)
        {
            _logger.LogInformation("Добавление нового назначения задачи {taskId} пользователю {userId}",
                                 entity.TaskId, entity.UserId);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Устанавливаем текущую дату, если не задана
                if (entity.AssignedAt == default)
                    entity.AssignedAt = DateTime.UtcNow;

                var model = entity.ToModel();
                return await _db.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении назначения задачи {taskId} пользователю {userId}",
                              entity?.TaskId, entity?.UserId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(TaskAssignation entity)
        {
            _logger.LogInformation("Обновление назначения задачи ID: {id}", entity.Id);
            try
            {
                if (entity == null)
                    return 0;

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении назначения задачи ID: {id}", entity?.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление назначения задачи ID: {id}", id);
            try
            {
                var assignation = await _db.TaskAssignations.FirstOrDefaultAsync(a => a.Id == id);
                if (assignation is null)
                    return 0;

                return await _db.DeleteAsync(assignation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении назначения задачи ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<TaskAssignation>> GetByTaskIdAsync(int taskId)
        {
            _logger.LogInformation("Получение назначений для задачи ID: {taskId}", taskId);
            try
            {
                var assignations = await _db.TaskAssignations
                    .Where(a => a.TaskId == taskId)
                    .OrderByDescending(a => a.AssignedAt)
                    .ToListAsync();

                return assignations.Select(a => a.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении назначений для задачи ID: {taskId}", taskId);
                throw;
            }
        }

        public async Task<IEnumerable<TaskAssignation>> GetByUserIdAsync(int userId)
        {
            _logger.LogInformation("Получение назначений для пользователя ID: {userId}", userId);
            try
            {
                var assignations = await _db.TaskAssignations
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.AssignedAt)
                    .ToListAsync();

                return assignations.Select(a => a.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении назначений для пользователя ID: {userId}", userId);
                throw;
            }
        }

        public async Task<TaskAssignation?> GetCurrentAssignmentAsync(int taskId)
        {
            _logger.LogInformation("Получение текущего назначения для задачи ID: {taskId}", taskId);
            try
            {
                var assignation = await _db.TaskAssignations
                    .Where(a => a.TaskId == taskId)
                    .OrderByDescending(a => a.AssignedAt)
                    .FirstOrDefaultAsync();

                return assignation?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении текущего назначения для задачи ID: {taskId}", taskId);
                throw;
            }
        }
    }
}