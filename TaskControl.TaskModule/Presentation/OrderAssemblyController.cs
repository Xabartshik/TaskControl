using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.Interface;

namespace TaskControl.TaskModule.Presentation
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderAssemblyController : ControllerBase
    {
        private readonly IOrderAssemblyExecutionService _executionService;
        private readonly ILogger<OrderAssemblyController> _logger;

        public OrderAssemblyController(
            IOrderAssemblyExecutionService executionService,
            ILogger<OrderAssemblyController> logger)
        {
            _executionService = executionService;
            _logger = logger;
        }

        // --- УДАЛЕННЫЕ ЭНДПОИНТЫ ---
        // GetHeaders (worker/{userId}/assignments) -> Используйте v1/WorkerTasks/{workerId}/pending
        // GetDetails (assignment/{id}/details)     -> Используйте v1/WorkerTasks/{taskId}/details
        // Start (assignment/{id}/start)            -> Используйте v1/WorkerTasks/{taskId}/start
        // Pause (assignment/{id}/pause)            -> Используйте v1/WorkerTasks/{taskId}/pause (если добавлено в агрегатор)
        // Cancel (assignment/{id}/cancel)          -> Используйте v1/WorkerTasks/{taskId}/cancel (если добавлено в агрегатор)
        // Complete (complete/{assignmentId})       -> Используйте v1/WorkerTasks/{taskId}/complete

        // --- ОСТАВЛЕННЫЕ УНИКАЛЬНЫЕ ЭНДПОИНТЫ ---

        [HttpPost("scan-pick")]
        public async Task<IActionResult> ScanAndPick([FromBody] ScanPickRequest req)
        {
            await _executionService.ScanAndPickItem(req.LineId, req.Barcode);
            return Ok();
        }

        [HttpPost("scan-place-bulk")]
        public async Task<IActionResult> ScanAndPlaceBulk([FromBody] ScanPlaceBulkRequest req)
        {
            var result = await _executionService.ScanAndPlaceBulk(req.AssignmentId, req.CellCode);
            return Ok(result);
        }

        [HttpPost("report-missing")]
        public async Task<IActionResult> ReportMissing([FromBody] ReportMissingRequest req)
        {
            await _executionService.ReportMissingItem(req.LineId, req.Reason);
            return Ok();
        }
    }

    public class ScanPickRequest { public int LineId { get; set; } public string Barcode { get; set; } }
    public class ScanPlaceBulkRequest { public int AssignmentId { get; set; } public string CellCode { get; set; } }
    public class ReportMissingRequest { public int LineId { get; set; } public string Reason { get; set; } }
}