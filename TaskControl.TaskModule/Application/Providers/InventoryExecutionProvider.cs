//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using TaskControl.TaskModule.Application.Interface;

//namespace TaskControl.TaskModule.Application.Providers
//{
//    public class InventoryExecutionProvider : ITaskExecutionProvider
//    {
//        private readonly ILogger<InventoryExecutionProvider> _logger;

//        public string TaskType => "inventory";

//        public InventoryExecutionProvider(ILogger<InventoryExecutionProvider> logger)
//        {
//            _logger = logger;
//        }

//        public Task PauseActiveTasksAsync(int workerId, int excludeTaskId)
//        {
//            _logger.LogInformation("InventoryExecutionProvider: PauseActiveTasksAsync no-op. WorkerId: {WorkerId}, ExcludeTaskId: {ExcludeTaskId}", workerId, excludeTaskId);
//            return Task.CompletedTask;
//        }

//        public Task<bool> TryActivateTaskAsync(int taskId, int workerId)
//        {
//            _logger.LogWarning("InventoryExecutionProvider: TryActivateTaskAsync not supported yet. TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
//            return Task.FromResult(false);
//        }

//        public Task<bool> TryCompleteAssignmentAsync(int taskId, int workerId)
//        {
//            _logger.LogWarning("InventoryExecutionProvider: TryCompleteAssignmentAsync not supported yet. TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
//            return Task.FromResult(false);
//        }

//        public Task<bool> TryPauseTaskAsync(int taskId, int workerId)
//        {
//            _logger.LogWarning("InventoryExecutionProvider: TryPauseTaskAsync not supported yet. TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
//            return Task.FromResult(false);
//        }

//        public Task<bool> TryCancelTaskAsync(int taskId, int workerId)
//        {
//            _logger.LogWarning("InventoryExecutionProvider: TryCancelTaskAsync not supported yet. TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
//            return Task.FromResult(false);
//        }

//        public Task<object?> GetTaskDetailsAsync(int taskId, int workerId)
//        {
//            _logger.LogWarning("InventoryExecutionProvider: GetTaskDetailsAsync not supported yet. TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
//            return Task.FromResult<object?>(null);
//        }

//        public Task<bool> IsTaskFullyCompletedAsync(int taskId)
//        {
//            _logger.LogWarning("InventoryExecutionProvider: IsTaskFullyCompletedAsync not supported yet. TaskId: {TaskId}", taskId);
//            return Task.FromResult(false);
//        }
//    }
//}
