using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;
using TaskControl.TaskModule.Domain;
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
        private readonly IOrderAssemblyExecutionService _orderAssemblyExecutionService;
        private readonly IInventoryProcessService _inventoryProcessService;

        public WorkerTasksController(
            TaskWorkloadAggregator aggregator,
            ITaskExecutionAggregator taskExecutionAggregator,
            IBaseTaskService baseTaskService,
            IOrderAssemblyExecutionService orderAssemblyExecutionService,
            IInventoryProcessService inventoryProcessService)
        {
            _taskWorkloadAggregator = aggregator;
            _taskExecutionAggregator = taskExecutionAggregator;
            _baseTaskService = baseTaskService;
            _orderAssemblyExecutionService = orderAssemblyExecutionService;
            _inventoryProcessService = inventoryProcessService;
        }

        [HttpGet("{workerId}/pending")]
        public async Task<ActionResult<IEnumerable<MobileBaseTaskDto>>> GetPendingTasks(int workerId)
        {
            var tasks = await _taskWorkloadAggregator.GetAllPendingTasksAsync(workerId);
            return Ok(tasks);
        }

        [HttpGet("{taskId}/details")]
        public async Task<IActionResult> GetTaskDetails(int taskId)
        {
            var baseTask = await _baseTaskService.GetById(taskId);
            if (baseTask == null) return NotFound("Задача не найдена.");

            return baseTask.Type switch
            {
                "Inventory" => Ok(await _inventoryProcessService.GetInventoryTaskDetailsAsync(taskId)),
                "OrderAssembly" => Ok(await _orderAssemblyExecutionService.GetAssemblyTaskDetailsAsync(taskId)),
                _ => BadRequest($"Получение деталей не поддерживается для типа задачи '{baseTask.Type}'.")
            };
        }

        [HttpPost("{taskId}/pause")]
        public async Task<IActionResult> PauseTask(int taskId)
        {
            var baseTask = await _baseTaskService.GetById(taskId);
            if (baseTask == null) return NotFound("Задача не найдена.");

            var success = baseTask.Type switch
            {
                "Inventory" => await _inventoryProcessService.PauseInventoryAsync(taskId),
                "OrderAssembly" => await _orderAssemblyExecutionService.PauseAssemblyAsync(taskId),
                _ => false
            };

            if (!success) return BadRequest("Не удалось поставить задачу на паузу.");
            return Ok();
        }

        [HttpPost("{taskId}/cancel")]
        public async Task<IActionResult> CancelTask(int taskId)
        {
            var baseTask = await _baseTaskService.GetById(taskId);
            if (baseTask == null) return NotFound("Задача не найдена.");

            var success = baseTask.Type switch
            {
                "Inventory" => await _inventoryProcessService.CancelInventoryAsync(taskId),
                "OrderAssembly" => await _orderAssemblyExecutionService.CancelAssemblyAsync(taskId),
                _ => false
            };

            if (!success) return BadRequest("Не удалось отменить задачу.");
            return Ok();
        }

        /// <summary>
        /// Начинает или продолжает выполнение задачи для указанного работника.
        /// Остальные активные задачи работника ставятся на паузу.
        /// </summary>
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
                return NotFound(new { message = $"Задача {taskId} не найдена или не может быть запущена данным работником." });
            }

            return Ok();
        }

        [HttpPost("{taskId}/complete")]
        public async Task<IActionResult> CompleteTask(int taskId, [FromQuery] int workerId)
        {
            var baseTask = await _baseTaskService.GetById(taskId);
            if (baseTask == null) return NotFound("Задача не найдена.");

            bool success = await _taskExecutionAggregator.CompleteAssignmentAsync(taskId, baseTask.Type, workerId);
            if (!success) return BadRequest("Не удалось завершить назначение.");

            bool isFullyCompleted = await _taskExecutionAggregator.IsTaskFullyCompletedAsync(taskId, baseTask.Type);

            if (isFullyCompleted)
            {
                var updatedTask = baseTask with
                {
                    Status = TaskStatus.Completed,
                    CompletedAt = DateTime.UtcNow
                };
                await _baseTaskService.Update(updatedTask);
                await ExecutePostCompletionLogicAsync(taskId, baseTask.Type);
            }

            return Ok(new { IsFullyCompleted = isFullyCompleted });
        }

        private async Task ExecutePostCompletionLogicAsync(int taskId, string taskType)
        {
            switch (taskType)
            {
                case "OrderAssembly":
                    await _orderAssemblyExecutionService.ApplyItemMovementsForCompletedTaskAsync(taskId);
                    break;

                case "Inventory":
                    break;

                case "Relocation":
                    break;

                default:
                    break;
            }
        }
    }
}
