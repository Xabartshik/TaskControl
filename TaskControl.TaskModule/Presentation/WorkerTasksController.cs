using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;
using TaskControl.TaskModule.DataAccess.Interface;
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
        private readonly IOrderAssemblyAssignmentRepository _orderAssemblyAssignmentRepository;
        private readonly ITaskDetailsBuilder _taskDetailsBuilder;

        public WorkerTasksController(TaskWorkloadAggregator aggregator, ITaskExecutionAggregator taskExecutionAggregator, IBaseTaskService baseTaskService, IOrderAssemblyExecutionService orderAssemblyExecutionService, IOrderAssemblyAssignmentRepository orderAssemblyAssignmentRepository, ITaskDetailsBuilder taskDetailsBuilder)
        {
            _taskWorkloadAggregator = aggregator;
            _taskExecutionAggregator = taskExecutionAggregator;
            _baseTaskService = baseTaskService;
            _orderAssemblyExecutionService = orderAssemblyExecutionService;
            _orderAssemblyAssignmentRepository = orderAssemblyAssignmentRepository;
            _taskDetailsBuilder = taskDetailsBuilder;
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
            var baseTask = await _baseTaskService.GetById(taskId);
            if (baseTask == null)
            {
                return NotFound(new { Message = $"Базовая задача с ID {taskId} не найдена." });
            }

            if (baseTask.Type != "OrderAssembly")
            {
                return BadRequest(new { Message = $"Тип задачи {baseTask.Type} пока не поддерживает details endpoint." });
            }

            var assignment = await _orderAssemblyAssignmentRepository.GetByTaskAndUserAsync(taskId, workerId);
            if (assignment == null)
            {
                return NotFound(new { Message = $"Назначение для задачи {taskId} и сотрудника {workerId} не найдено." });
            }

            var dto = new MobileBaseTaskDto
            {
                TaskId = assignment.TaskId,
                Title = baseTask.Title ?? $"Сборка заказа #{assignment.OrderId}",
                TaskType = baseTask.Type,
                PriorityLevel = baseTask.PriorityLevel,
                Status = baseTask.Status,
                AssignmentStatus = assignment.Status,
                CreatedAt = assignment.AssignedAt,
                Deadline = baseTask.Deadline,
                TaskDetails = _taskDetailsBuilder.BuildOrderAssemblyDetails(assignment)
            };

            return Ok(dto);
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
            // Вызываем метод и получаем результат (успех/провал)
            bool isStarted = await _taskExecutionAggregator.StartOrResumeTaskAsync(taskId, workerId);
            // Если это первый человек, который нажал "Начать", задача переходит в работу для всех.
            if (baseTask.Status == TaskStatus.New || baseTask.Status == TaskStatus.Assigned)
            {
                var updatedTask = baseTask with { Status = TaskStatus.InProgress };
                await _baseTaskService.Update(updatedTask);
            }
            if (!isStarted)
            {
                // Возвращаем ошибку, если задача не найдена или не принадлежит работнику
                return NotFound(new { message = $"Задача {taskId} не найдена или не может быть запущена данным работником." });
            }

            return Ok();
        }

        [HttpPost("{taskId}/complete")]
        public async Task<IActionResult> CompleteTask(int taskId, [FromQuery] int workerId)
        {
            var baseTask = await _baseTaskService.GetById(taskId);
            if (baseTask == null) return NotFound("Задача не найдена.");

            // 1. Закрываем назначение для конкретного человека (и помощника, если это главный)
            bool success = await _taskExecutionAggregator.CompleteAssignmentAsync(taskId, baseTask.Type, workerId);
            if (!success) return BadRequest("Не удалось завершить назначение.");

            // 2. Проверяем, закрыта ли задача полностью
            bool isFullyCompleted = await _taskExecutionAggregator.IsTaskFullyCompletedAsync(taskId, baseTask.Type);

            if (isFullyCompleted)
            {
                // 3. Закрываем глобальную задачу
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
                    // Применяем движения товаров после сборки заказа
                    await _orderAssemblyExecutionService.ApplyItemMovementsForCompletedTaskAsync(taskId);
                    break;

                case "Inventory":
                    // Задел на будущее: если после инвентаризации нужно, 
                    // например, списать недостачи или обновить статусы ячеек
                    // await _inventoryProcessService.ApplyInventoryResultsAsync(taskId);
                    break;

                case "Relocation":
                    // Задел на будущее: перемещение между зонами
                    // await _relocationService.CompleteRelocationAsync(taskId);
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
