using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.Providers;

namespace TaskControl.TaskModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")] // Используем версионирование, как у тебя в WorkerTasksController
    public class OrderHandoverController : ControllerBase
    {
        private readonly OrderHandoverExecutionProvider _provider;

        public OrderHandoverController(OrderHandoverExecutionProvider provider)
        {
            _provider = provider;
        }

        // Этот метод уникален только для процесса выдачи!
        [HttpPost("{taskId}/scan")]
        public async Task<IActionResult> Scan(int taskId, [FromQuery] int workerId, [FromBody] string barcode)
        {
            var (success, message) = await _provider.ProcessScanAsync(taskId, workerId, barcode);
            if (!success) return BadRequest(new { Error = message });

            return Ok(new { Message = message });
        }
    }
}