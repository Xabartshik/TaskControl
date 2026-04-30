using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services
{
    public class TaskExecutionAggregator : ITaskExecutionAggregator
    {
        private readonly IEnumerable<ITaskExecutionProvider> _executionProviders;
        private readonly ILogger<TaskExecutionAggregator> _logger;

        public TaskExecutionAggregator(
            IEnumerable<ITaskExecutionProvider> executionProviders,
            ILogger<TaskExecutionAggregator> logger)
        {
            _executionProviders = executionProviders;
            _logger = logger;
        }

        public async Task<bool> StartOrResumeTaskAsync(int taskId, int workerId)
        {
            _logger.LogInformation("Запуск процесса переключения задачи. Целевая задача: {TaskId}, Работник: {WorkerId}", taskId, workerId);

            // 1. Ставим на паузу все текущие активные назначения работника
            foreach (var provider in _executionProviders)
            {
                await provider.PauseActiveTasksAsync(workerId, taskId);
            }

            bool activated = false;

            // 2. Ищем целевое назначение и активируем его
            foreach (var provider in _executionProviders)
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

        //public async Task<bool> StartTaskAsync(int taskId, string taskType, int workerId)
        //{
        //    var provider = _executionProviders.FirstOrDefault(p => p.TaskType == taskType);

        //    if (provider == null)
        //        throw new ArgumentException($"Провайдер для типа задачи '{taskType}' не зарегистрирован в системе.");

        //    return await provider.TryStartTaskAsync(taskId, workerId);
        //}

        public async Task<bool> CompleteAssignmentAsync(int taskId, string taskType, int workerId)
        {
            var provider = _executionProviders.FirstOrDefault(p => p.TaskType == taskType);
            if (provider == null) return false;

            return await provider.TryCompleteAssignmentAsync(taskId, workerId);
        }

        public async Task<bool> IsTaskFullyCompletedAsync(int taskId, string taskType)
        {
            var provider = _executionProviders.FirstOrDefault(p => p.TaskType == taskType);
            if (provider == null) return false;

            return await provider.IsTaskFullyCompletedAsync(taskId);
        }
    }
}