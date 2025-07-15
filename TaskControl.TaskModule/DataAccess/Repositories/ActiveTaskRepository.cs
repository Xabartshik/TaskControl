using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.SharedInterfaces;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DAL.Repositories
{
    public class ActiveTaskRepository : IRepository<ActiveTask>
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<ActiveTaskRepository> _logger;

        public ActiveTaskRepository(ITaskDataConnection db, ILogger<ActiveTaskRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<ActiveTask?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск задачи по ID: {id}", id);
            try
            {
                var task = await _db.ActiveTasks.FirstOrDefaultAsync(t => t.TaskId == id);
                return task?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задачи по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ActiveTask>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех задач");
            try
            {
                var tasks = await _db.ActiveTasks.ToListAsync();
                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка задач");
                throw;
            }
        }

        public async Task<int> AddAsync(ActiveTask entity)
        {
            _logger.LogInformation("Добавление новой задачи типа {type} в филиал {branchId}",
                                entity.Type, entity.BranchId);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Валидация статуса задачи
                if (!new[] { "New", "InProgress", "Completed", "Cancelled" }.Contains(entity.Status))
                    throw new ArgumentException("Недопустимый статус задачи");

                var model = entity.ToModel();
                return await _db.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении задачи типа {type}", entity?.Type);
                throw;
            }
        }

        public async Task<int> UpdateAsync(ActiveTask entity)
        {
            _logger.LogInformation("Обновление задачи ID: {taskId}", entity.TaskId);
            try
            {
                if (entity == null)
                    return 0;

                // Валидация статуса задачи
                if (!new[] { "New", "InProgress", "Completed", "Cancelled" }.Contains(entity.Status))
                    throw new ArgumentException("Недопустимый статус задачи");

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении задачи ID: {taskId}", entity?.TaskId);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление задачи ID: {id}", id);
            try
            {
                var task = await _db.ActiveTasks.FirstOrDefaultAsync(t => t.TaskId == id);
                if (task is null)
                    return 0;

                return await _db.DeleteAsync(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении задачи ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ActiveTask>> GetByBranchAsync(int branchId)
        {
            _logger.LogInformation("Получение задач филиала ID: {branchId}", branchId);
            try
            {
                var tasks = await _db.ActiveTasks
                    .Where(t => t.BranchId == branchId)
                    .ToListAsync();

                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задач филиала ID: {branchId}", branchId);
                throw;
            }
        }

        public async Task<IEnumerable<ActiveTask>> GetByStatusAsync(string status)
        {
            _logger.LogInformation("Получение задач со статусом: {status}", status);
            try
            {
                var tasks = await _db.ActiveTasks
                    .Where(t => t.Status == status)
                    .ToListAsync();

                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задач со статусом: {status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<ActiveTask>> GetActiveTasksAsync()
        {
            _logger.LogInformation("Получение активных задач");
            try
            {
                var tasks = await _db.ActiveTasks
                    .Where(t => t.Status != "Completed" && t.Status != "Cancelled")
                    .ToListAsync();

                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении активных задач");
                throw;
            }
        }

        public async Task<IEnumerable<ActiveTask>> GetByTypeAsync(string type)
        {
            _logger.LogInformation("Получение задач типа: {type}", type);
            try
            {
                var tasks = await _db.ActiveTasks
                    .Where(t => t.Type == type)
                    .ToListAsync();

                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задач типа: {type}", type);
                throw;
            }
        }
    }
}