using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class WorkerTasksController : ControllerBase
    {
        private readonly TaskWorkloadAggregator _taskWorkloadAggregator;
        private readonly ITaskExecutionAggregator _taskExecutionAggregator;
        private readonly IBaseTaskService _baseTaskService;

        public WorkerTasksController(
            TaskWorkloadAggregator aggregator,
            ITaskExecutionAggregator taskExecutionAggregator,
            IBaseTaskService baseTaskService)
        {
            _taskWorkloadAggregator = aggregator;
            _taskExecutionAggregator = taskExecutionAggregator;
            _baseTaskService = baseTaskService;
        }

        [HttpGet("{workerId}/pending")]
        public async Task<ActionResult<IEnumerable<MobileBaseTaskDto>>> GetPendingTasks(int workerId)
        {
            var tasks = await _taskWorkloadAggregator.GetAllPendingTasksAsync(workerId);
            return Ok(tasks);
        }

        [HttpGet("{workerId}/{taskId}/details")]
        public async Task<ActionResult<MobileBaseTaskDto>> GetDetails(int workerId, int taskId)
        {
            // Агрегатор сам найдет провайдера и вызовет его метод GetTaskDetailsAsync
            var taskDto = await _taskWorkloadAggregator.GetTaskDetailsAsync(taskId, workerId);

            if (taskDto == null)
            {
                return NotFound(new { Message = $"Задача с ID {taskId} не найдена или недоступна." });
            }

            return Ok(taskDto);
        }

        [HttpPost("{taskId}/start")]
        public async Task<IActionResult> StartTask(int taskId, [FromQuery] int workerId)
        {
            var baseTask = await _baseTaskService.GetById(taskId);
            if (baseTask == null)
            {
                return NotFound(new { Message = $"Базовая задача с ID {taskId} не найдена." });
            }

            bool isStarted = await _taskExecutionAggregator.StartOrResumeTaskAsync(taskId, workerId);

            if (baseTask.Status == TaskStatus.New || baseTask.Status == TaskStatus.Assigned)
            {
                var updatedTask = baseTask with { Status = TaskStatus.InProgress };
                await _baseTaskService.Update(updatedTask);
            }

            if (!isStarted)
            {
                return NotFound(new { Message = $"Задача {taskId} не найдена или не может быть запущена данным работником." });
            }

            return Ok();
        }

        [HttpPost("{taskId}/pause")]
        public async Task<IActionResult> PauseTask(int taskId, [FromQuery] int workerId)
        {
            var baseTask = await _baseTaskService.GetById(taskId);
            if (baseTask == null) return NotFound(new { Message = $"Базовая задача с ID {taskId} не найдена." });

            var success = await _taskExecutionAggregator.PauseTaskAsync(taskId, baseTask.Type, workerId);
            if (!success) return BadRequest(new { Message = $"Не удалось поставить на паузу задачу {taskId} для работника {workerId}." });

            return Ok();
        }

        [HttpPost("{taskId}/cancel")]
        public async Task<IActionResult> CancelTask(int taskId, [FromQuery] int workerId)
        {
            var baseTask = await _baseTaskService.GetById(taskId);
            if (baseTask == null) return NotFound(new { Message = $"Базовая задача с ID {taskId} не найдена." });

            var success = await _taskExecutionAggregator.CancelTaskAsync(taskId, baseTask.Type, workerId);
            if (!success) return BadRequest(new { Message = $"Не удалось отменить задачу {taskId} для работника {workerId}." });

            return Ok();
        }

        [HttpPost("{taskId}/complete")]
        public async Task<IActionResult> CompleteTask(int taskId, [FromQuery] int workerId)
        {
            var baseTask = await _baseTaskService.GetById(taskId);
            if (baseTask == null) return NotFound(new { Message = $"Базовая задача с ID {taskId} не найдена." });

            bool success = await _taskExecutionAggregator.CompleteAssignmentAsync(taskId, baseTask.Type, workerId);
            if (!success) return BadRequest(new { Message = $"Не удалось завершить назначение для задачи {taskId} и работника {workerId}." });

            bool isFullyCompleted = await _taskExecutionAggregator.IsTaskFullyCompletedAsync(taskId, baseTask.Type);

            if (isFullyCompleted)
            {
                var updatedTask = baseTask with
                {
                    Status = TaskStatus.Completed,
                    CompletedAt = DateTime.UtcNow
                };
                await _baseTaskService.Update(updatedTask);

                // Делегируем выполнение пост-логики конкретному Execution-провайдеру
                await _taskExecutionAggregator.ExecutePostCompletionLogicAsync(taskId, baseTask.Type);
            }

            return Ok(new { IsFullyCompleted = isFullyCompleted });
        }
                    break;

                default:
                    // Если для типа задачи нет специфичной складской логики завершения — просто ничего не делаем
                    // Можно добавить логгер, если хотите отслеживать такие случаи:
                    // _logger.LogInformation("Для типа задачи {TaskType} не предусмотрена специфичная логика завершения", taskType);
                    break;
            }
        }
    }
}