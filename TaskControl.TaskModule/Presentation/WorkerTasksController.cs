using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;

namespace TaskControl.TaskModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class WorkerTasksController : ControllerBase
    {
        private readonly TaskWorkloadAggregator _aggregator;
        private readonly ITaskExecutionAggregator _taskExecutionAggregator;
        private readonly IBaseTaskService _baseTaskService;

        public WorkerTasksController(TaskWorkloadAggregator aggregator, ITaskExecutionAggregator taskExecutionAggregator, IBaseTaskService baseTaskService)
        {
            _aggregator = aggregator;
            _taskExecutionAggregator = taskExecutionAggregator;
            _baseTaskService = baseTaskService;
        }

        [HttpGet("{workerId}/pending")]
        public async Task<ActionResult<IEnumerable<MobileBaseTaskDto>>> GetPendingTasks(int workerId)
        {
            var tasks = await _aggregator.GetAllPendingTasksAsync(workerId);
            return Ok(tasks);
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

            if (!isStarted)
            {
                // Возвращаем ошибку, если задача не найдена или не принадлежит работнику
                return NotFound(new { message = $"Задача {taskId} не найдена или не может быть запущена данным работником." });
            }

            return Ok();
        }
    }
}