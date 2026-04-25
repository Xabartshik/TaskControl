using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Services;

namespace TaskControl.TaskModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkerTasksController : ControllerBase
    {
        private readonly TaskWorkloadAggregator _aggregator;

        public WorkerTasksController(TaskWorkloadAggregator aggregator)
        {
            _aggregator = aggregator;
        }

        [HttpGet("{workerId}/pending")]
        public async Task<ActionResult<IEnumerable<MobileBaseTaskDto>>> GetPendingTasks(int workerId)
        {
            var tasks = await _aggregator.GetAllPendingTasksAsync(workerId);
            return Ok(tasks);
        }
    }
}