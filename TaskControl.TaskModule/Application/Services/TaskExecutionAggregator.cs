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
        private readonly ITaskDataConnection _dataConnection;

        public TaskExecutionAggregator(
            IEnumerable<ITaskStateProvider> stateProviders,
            ITaskDataConnection dataConnection)
        {
            _stateProviders = stateProviders;
            _dataConnection = dataConnection;
        }

        public async Task StartOrResumeTaskAsync(int taskId, int workerId)
        {
            // 1. Ставим на паузу все текущие активные назначения работника во всех модулях
            foreach (var provider in _stateProviders)
            {
                await provider.PauseActiveTasksAsync(workerId, taskId);
            }

            // 2. Ищем целевое назначение и активируем его
            foreach (var provider in _stateProviders)
            {
                // TryActivateTaskAsync внутри себя проверяет, принадлежит ли назначение этому работнику,
                // и если да, меняет статус и сохраняет изменения в БД
                if (await provider.TryActivateTaskAsync(taskId, workerId))
                {
                    break; // Как только нашли и активировали — прерываем цикл, дальше искать не нужно
                }
            }
        }
    }
}