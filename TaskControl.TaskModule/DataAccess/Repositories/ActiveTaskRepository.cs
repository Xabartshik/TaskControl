using LinqToDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.DAL.Repositories
{
    public class ActiveTaskRepository : IRepository<BaseTask>, IActiveTaskRepository
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<ActiveTaskRepository> _logger;

        public ActiveTaskRepository(ITaskDataConnection db, ILogger<ActiveTaskRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<BaseTask> GetByIdAsync(int id)
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

        public async Task<IEnumerable<BaseTask>> GetAllAsync()
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

        public async Task<int> AddAsync(BaseTask entity)
        {
            _logger.LogInformation("Добавление новой задачи: '{Title}' типа {Type} в филиал {BranchId}",
                entity.Title, entity.Type, entity.BranchId);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Валидация статуса задачи
                if (!Enum.IsDefined(typeof(TaskStatus), entity.Status))
                    throw new ArgumentException($"Недопустимый статус задачи: {entity.Status}");

                // Валидация приоритета
                if (entity.Priority < 0 || entity.Priority > 10)
                    throw new ArgumentException($"Недопустимый приоритет задачи: {entity.Priority}. Должен быть от 0 до 10.");

                var model = entity.ToModel();
                var taskId = await _db.InsertAsync(model);

                _logger.LogInformation("Задача '{Title}' успешно добавлена с ID: {TaskId}", entity.Title, taskId);
                return taskId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении задачи '{Title}' типа {Type}", entity?.Title, entity?.Type);
                throw;
            }
        }

        public async Task<int> UpdateAsync(BaseTask entity)
        {
            _logger.LogInformation("Обновление задачи ID: {TaskId}, статус: {Status}", entity.TaskId, entity.Status);
            try
            {
                if (entity == null)
                    return 0;

                // Валидация статуса задачи
                if (!Enum.IsDefined(typeof(TaskStatus), entity.Status))
                    throw new ArgumentException($"Недопустимый статус задачи: {entity.Status}");

                // Валидация приоритета
                if (entity.Priority < 0 || entity.Priority > 10)
                    throw new ArgumentException($"Недопустимый приоритет задачи: {entity.Priority}. Должен быть от 0 до 10.");

                var model = entity.ToModel();
                var result = await _db.UpdateAsync(model);

                _logger.LogInformation("Задача ID: {TaskId} успешно обновлена", entity.TaskId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении задачи ID: {TaskId}", entity?.TaskId);
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
                {
                    _logger.LogWarning("Задача ID: {id} не найдена для удаления", id);
                    return 0;
                }

                var result = await _db.DeleteAsync(task);
                _logger.LogInformation("Задача ID: {id} успешно удалена", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении задачи ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<BaseTask>> GetByBranchAsync(int branchId)
        {
            _logger.LogInformation("Получение задач филиала ID: {branchId}", branchId);
            try
            {
                var tasks = await _db.ActiveTasks
                    .Where(t => t.BranchId == branchId)
                    .ToListAsync();

                _logger.LogInformation("Найдено {Count} задач для филиала ID: {branchId}", tasks.Count, branchId);
                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задач филиала ID: {branchId}", branchId);
                throw;
            }
        }

        /// <summary>
        /// Получить задачи по статусу (используя enum)
        /// </summary>
        public async Task<IEnumerable<BaseTask>> GetByStatusAsync(TaskStatus status)
        {
            _logger.LogInformation("Получение задач со статусом: {Status}", status);
            try
            {
                var statusString = status.ToString();

                var tasks = await _db.ActiveTasks
                    .Where(t => t.Status == statusString)
                    .ToListAsync();

                _logger.LogInformation("Найдено {Count} задач со статусом: {Status}", tasks.Count, status);
                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задач со статусом: {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Получить только активные задачи (не завершенные и не отмененные)
        /// </summary>
        public async Task<IEnumerable<BaseTask>> GetActiveTasksAsync()
        {
            _logger.LogInformation("Получение активных задач");
            try
            {
                var completedStatus = TaskStatus.Completed.ToString(); 
                var cancelledStatus = TaskStatus.Cancelled.ToString(); 

                var tasks = await _db.ActiveTasks
                    .Where(t => t.Status != completedStatus && t.Status != cancelledStatus)
                    .ToListAsync();

                _logger.LogInformation("Найдено {Count} активных задач", tasks.Count);
                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении активных задач");
                throw;
            }
        }

        public async Task<IEnumerable<BaseTask>> GetByTypeAsync(string type)
        {
            _logger.LogInformation("Получение задач типа: {Type}", type);
            try
            {
                var tasks = await _db.ActiveTasks
                    .Where(t => t.Type == type)
                    .ToListAsync();

                _logger.LogInformation("Найдено {Count} задач типа: {Type}", tasks.Count, type);
                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задач типа: {Type}", type);
                throw;
            }
        }

        /// <summary>
        /// Получить задачи по приоритету
        /// </summary>
        public async Task<IEnumerable<BaseTask>> GetByPriorityAsync(int priority)
        {
            _logger.LogInformation("Получение задач с приоритетом: {Priority}", priority);
            try
            {
                if (priority < 0 || priority > 10)
                    throw new ArgumentException($"Приоритет должен быть от 0 до 10, получено: {priority}");

                var tasks = await _db.ActiveTasks
                    .Where(t => t.Priority == priority)
                    .ToListAsync();

                _logger.LogInformation("Найдено {Count} задач с приоритетом: {Priority}", tasks.Count, priority);
                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задач с приоритетом: {Priority}", priority);
                throw;
            }
        }

        /// <summary>
        /// Получить задачи по филиалу и статусу
        /// </summary>
        public async Task<IEnumerable<BaseTask>> GetByBranchAndStatusAsync(int branchId, TaskStatus status)
        {
            _logger.LogInformation("Получение задач филиала ID: {BranchId} со статусом: {Status}", branchId, status);
            try
            {
                var statusString = status.ToString();

                var tasks = await _db.ActiveTasks
                    .Where(t => t.BranchId == branchId && t.Status == statusString)
                    .ToListAsync();

                _logger.LogInformation("Найдено {Count} задач для филиала ID: {BranchId} со статусом: {Status}",
                    tasks.Count, branchId, status);

                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задач филиала ID: {BranchId} со статусом: {Status}",
                    branchId, status);
                throw;
            }
        }

        /// <summary>
        /// Получить задачи с высоким приоритетом (7-10)
        /// </summary>
        public async Task<IEnumerable<BaseTask>> GetHighPriorityTasksAsync()
        {
            _logger.LogInformation("Получение задач с высоким приоритетом");
            try
            {
                var tasks = await _db.ActiveTasks
                    .Where(t => t.Priority >= 7)
                    .OrderByDescending(t => t.Priority)
                    .ToListAsync();

                _logger.LogInformation("Найдено {Count} задач с высоким приоритетом", tasks.Count);
                return tasks.Select(t => t.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задач с высоким приоритетом");
                throw;
            }
        }

        /// <summary>
        /// Обновить статус задачи
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int taskId, TaskStatus newStatus)
        {
            _logger.LogInformation("Обновление статуса задачи ID: {TaskId} на {NewStatus}", taskId, newStatus);
            try
            {
                if (!Enum.IsDefined(typeof(TaskStatus), newStatus))
                    throw new ArgumentException($"Недопустимый статус задачи: {newStatus}");

                var task = await _db.ActiveTasks.FirstOrDefaultAsync(t => t.TaskId == taskId);
                if (task == null)
                {
                    _logger.LogWarning("Задача ID: {TaskId} не найдена", taskId);
                    return false;
                }

                task.Status = newStatus.ToString();

                // Если задача завершена или отменена, устанавливаем дату завершения
                if (newStatus == TaskStatus.Completed || newStatus == TaskStatus.Cancelled)
                {
                    task.CompletedAt = DateTime.UtcNow;
                }

                await _db.UpdateAsync(task);
                _logger.LogInformation("Статус задачи ID: {TaskId} успешно обновлен на {NewStatus}", taskId, newStatus);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении статуса задачи ID: {TaskId}", taskId);
                throw;
            }
        }
    }
}
