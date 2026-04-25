using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services
{
    public class TaskExecutionAggregator : ITaskExecutionAggregator
    {
        private readonly IEnumerable<ITaskStateProvider> _stateProviders;
        private readonly ILogger<TaskExecutionAggregator> _logger;

        public TaskExecutionAggregator(
            IEnumerable<ITaskStateProvider> stateProviders,
            ILogger<TaskExecutionAggregator> logger)
        {
            _stateProviders = stateProviders;
            _logger = logger;
        }

        public async Task<bool> StartOrResumeTaskAsync(int taskId, int workerId)
        {
            _logger.LogInformation("Запуск процесса переключения задачи. Целевая задача: {TaskId}, Работник: {WorkerId}", taskId, workerId);

            // 1. Ставим на паузу все текущие активные назначения работника
            foreach (var provider in _stateProviders)
            {
                await provider.PauseActiveTasksAsync(workerId, taskId);
            }

            bool activated = false;

            // 2. Ищем целевое назначение и активируем его
            foreach (var provider in _stateProviders)
            {
                if (await provider.TryActivateTaskAsync(taskId, workerId))
                {
                    activated = true;
                    _logger.LogInformation("Задача {TaskId} успешно активирована для работника {WorkerId}", taskId, workerId);
                    break;
                }
            }

            if (!activated)
            {
                _logger.LogWarning("Не удалось найти назначение для задачи {TaskId} у пользователя {WorkerId}", taskId, workerId);
            }

            return activated;
        }
    }
}