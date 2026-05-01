using System; // Добавлено для DateTime
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.Application.Providers
{
    public class OrderAssemblyExecutionProvider : ITaskExecutionProvider
    {
        private readonly IOrderAssemblyAssignmentRepository _assemblyRepo;
        private readonly IOrderAssemblyExecutionService _orderAssemblyExecutionService;
        private readonly IBaseTaskService _baseTaskService;
        private readonly ILogger<OrderAssemblyExecutionProvider> _logger;

        public string TaskType => "OrderAssembly";

        public OrderAssemblyExecutionProvider(
            IOrderAssemblyAssignmentRepository assemblyRepo,
            IOrderAssemblyExecutionService orderAssemblyExecutionService,
            IBaseTaskService baseTaskService,
            ILogger<OrderAssemblyExecutionProvider> logger)
        {
            _assemblyRepo = assemblyRepo;
            _orderAssemblyExecutionService = orderAssemblyExecutionService;
            _baseTaskService = baseTaskService;
            _logger = logger;
        }

        public async Task<bool> TryCompleteAssignmentAsync(int taskId, int workerId)
        {
            _logger.LogInformation("Попытка завершения назначения. TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);

            // Получаем ВСЕ назначения для этой задачи (и Главного, и Помощника)
            var allAssignments = await _assemblyRepo.GetAllByTaskIdAsync(taskId);

            // Находим назначение того, кто нажал кнопку
            var currentAssignment = allAssignments.FirstOrDefault(a => a.AssignedToUserId == workerId);

            if (currentAssignment == null)
            {
                _logger.LogWarning("Назначение не найдено для TaskId: {TaskId} и WorkerId: {WorkerId}", taskId, workerId);
                return false;
            }

            // Безопасное время для PostgreSQL
            var completionTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            // 1. Завершаем назначение текущего сотрудника
            currentAssignment.Status = (int)AssignmentStatus.Completed;


            currentAssignment.CompletedAt = completionTime;

            await _assemblyRepo.UpdateAsync(currentAssignment.ToDomain());
            _logger.LogDebug("Назначение ID: {AssignmentId} переведено в статус Completed", currentAssignment.Id);

            // 2. Если на кнопку нажал ГЛАВНЫЙ работник, мы автоматически гасим назначение помощника
            if (currentAssignment.Role == (int)Domain.AssignmentRole.Main)
            {
                _logger.LogInformation("Главный сборщик завершил работу. Проверка наличия помощников для TaskId: {TaskId}", taskId);
                var helperAssignment = allAssignments.FirstOrDefault(a => a.Role == (int)Domain.AssignmentRole.Helper);

                if (helperAssignment != null && helperAssignment.Status != (int)AssignmentStatus.Completed)
                {
                    helperAssignment.Status = (int)AssignmentStatus.Completed;


                    helperAssignment.CompletedAt = completionTime;

                    await _assemblyRepo.UpdateAsync(helperAssignment.ToDomain());
                    _logger.LogInformation("Назначение помощника ID: {HelperAssignmentId} автоматически завершено", helperAssignment.Id);
                }
            }

            _logger.LogInformation("Магия склада для TaskId: {TaskId} успешно выполнена", taskId);
            return true;
        }

        public async Task<bool> IsTaskFullyCompletedAsync(int taskId)
        {
            var allAssignmentsForTask = await _assemblyRepo.GetAllByTaskIdAsync(taskId);

            if (allAssignmentsForTask == null || !allAssignmentsForTask.Any())
            {
                _logger.LogWarning("Проверка завершения задачи: назначения для TaskId: {TaskId} отсутствуют", taskId);
                return false;
            }

            // Ищем, есть ли еще назначения в работе или ожидании
            bool hasUnfinished = allAssignmentsForTask.Any(a =>
                (int)a.Status == 0 || (int)a.Status == 1); // 0 = Assigned, 1 = InProgress

            bool isDone = !hasUnfinished;
            _logger.LogInformation("Проверка полноты выполнения TaskId: {TaskId}. Результат: {IsDone}", taskId, isDone);

            return isDone;
        }

        public async Task<bool> TryPauseTaskAsync(int taskId, int workerId)
        {
            var assignment = await _assemblyRepo.GetByTaskAndUserAsync(taskId, workerId);
            if (assignment == null)
            {
                _logger.LogWarning("Пауза невозможна: назначение не найдено. TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
                return false;
            }

            assignment.Status = (int)AssignmentStatus.Paused;
            await _assemblyRepo.UpdateAsync(assignment.ToDomain());
            _logger.LogInformation("Назначение поставлено на паузу. TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
            return true;
        }

        public async Task<bool> TryCancelTaskAsync(int taskId, int workerId)
        {
            var assignment = await _assemblyRepo.GetByTaskAndUserAsync(taskId, workerId);
            if (assignment == null)
            {
                _logger.LogWarning("Отмена невозможна: назначение не найдено. TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
                return false;
            }

            assignment.Status = (int)AssignmentStatus.Cancelled;
            await _assemblyRepo.UpdateAsync(assignment.ToDomain());
            _logger.LogInformation("Назначение отменено. TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
            return true;
        }

        public async Task<object?> GetTaskDetailsAsync(int taskId, int workerId)
        {
            var assignment = await _assemblyRepo.GetByTaskAndUserAsync(taskId, workerId);
            if (assignment == null)
            {
                _logger.LogWarning("Не удалось получить детали назначения. TaskId: {TaskId}, WorkerId: {WorkerId}", taskId, workerId);
                return null;
            }

            return assignment;
        }

        //public async Task<bool> TryStartTaskAsync(int taskId, int workerId)
        //{
        //    _logger.LogInformation("Попытка старта задачи TaskId: {TaskId} пользователем WorkerId: {WorkerId}", taskId, workerId);

        //    var userAssignments = await _assemblyRepo.GetByUserIdAsync(workerId);
        //    var assignment = userAssignments.FirstOrDefault(a => a.TaskId == taskId);

        //    if (assignment == null)
        //    {
        //        _logger.LogWarning("Старт невозможен: назначение для TaskId: {TaskId} у пользователя {WorkerId} не найдено", taskId, workerId);
        //        return false;
        //    }

        //    _logger.LogInformation("Задача TaskId: {TaskId} успешно переведена в статус InProgress для WorkerId: {WorkerId}", taskId, workerId);
        //    return true;
        //}

        public async Task PauseActiveTasksAsync(int workerId, int excludeTaskId)
        {
            _logger.LogInformation("Поиск активных сборок заказа для постановки на паузу. Пользователь: {WorkerId}", workerId);

            var assignments = await _assemblyRepo.GetByUserIdAsync(workerId);

            var activeAssignments = assignments.Where(a =>
                a.Status == AssignmentStatus.InProgress && a.TaskId != excludeTaskId).ToList();

            if (!activeAssignments.Any())
            {
                _logger.LogDebug("Активных сборок для пользователя {WorkerId} не найдено.", workerId);
                return;
            }

            foreach (var assignment in activeAssignments)
            {
                _logger.LogInformation("Ставим на паузу сборку (Assignment ID: {Id}) для задачи {TaskId}", assignment.Id, assignment.TaskId);
                assignment.Status = AssignmentStatus.Paused;
                await _assemblyRepo.UpdateAsync(assignment);
            }
        }

        public async Task<bool> TryActivateTaskAsync(int taskId, int workerId)
        {
            var assignment = await _assemblyRepo.GetByTaskAndUserAsync(taskId, workerId);

            if (assignment != null)
            {
                _logger.LogInformation("Активация сборки заказа. TaskID: {TaskId}, WorkerID: {WorkerId}", taskId, workerId);
                assignment.Status = (int)AssignmentStatus.InProgress;
                // Безопасное время для PostgreSQL
                var startTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                assignment.StartedAt = startTime;
                await _assemblyRepo.UpdateAsync(assignment.ToDomain());
                return true;
            }

            _logger.LogWarning("Не удалось активировать задачу. TaskID: {TaskId}, WorkerID: {WorkerId} - назначение не найдено", taskId, workerId);
            return false;
        }

        public async Task<bool> TryPauseTaskAsync(int taskId, int workerId)
        {
            var assignment = await _assemblyRepo.GetByTaskAndUserAsync(taskId, workerId);
            if (assignment == null)
            {
                return false;
            }

            assignment.Status = (int)AssignmentStatus.Paused;
            await _assemblyRepo.UpdateAsync(assignment.ToDomain());
            return true;
        }

        public async Task<bool> TryCancelTaskAsync(int taskId, int workerId)
        {
            var assignment = await _assemblyRepo.GetByTaskAndUserAsync(taskId, workerId);
            if (assignment == null)
            {
                return false;
            }

            assignment.Status = (int)AssignmentStatus.Cancelled;
            await _assemblyRepo.UpdateAsync(assignment.ToDomain());
            return true;
        }
    }
}
