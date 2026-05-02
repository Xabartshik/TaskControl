using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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

        private void AddDeprecationWarning(string replacement)
        {
            Response.Headers.Append("Warning", $"299 - Deprecated API. Use {replacement}");
            Response.Headers.Append("X-Api-Deprecated", "true");
        }

        /// <summary>DEPRECATED: используйте GET /api/v1/WorkerTasks/{workerId}/pending.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{workerId}/pending")]
        [HttpGet("worker/{userId}/assignments")]
        public async Task<IActionResult> GetHeaders(int userId)
        {
            AddDeprecationWarning("GET /api/v1/WorkerTasks/{workerId}/pending");
            return Ok(await _executionService.GetAssignmentsHeaderForWorkerAsync(userId));
        }

        /// <summary>DEPRECATED: используйте GET /api/v1/WorkerTasks/{taskId}/details.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/details")]
        [HttpGet("assignment/{id}/details")]
        public async Task<IActionResult> GetDetails(int id)
        {
            AddDeprecationWarning("GET /api/v1/WorkerTasks/{taskId}/details");
            return Ok(await _executionService.GetAssemblyTaskDetailsAsync(id));
        }

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/start?workerId=...</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/start")]
        [HttpPost("assignment/{id}/start")]
        public async Task<IActionResult> Start(int id)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/start?workerId=...");
            return Ok(await _executionService.StartAssemblyAsync(id));
        }

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/pause.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/pause")]
        [HttpPost("assignment/{id}/pause")]
        public async Task<IActionResult> Pause(int id)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/pause");
            return Ok(await _executionService.PauseAssemblyAsync(id));
        }

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/cancel.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/cancel")]
        [HttpPost("assignment/{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/cancel");
            return Ok(await _executionService.CancelAssemblyAsync(id));
        }

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/complete?workerId=...</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/complete")]
        [HttpPost("complete/{assignmentId}")]
        public async Task<IActionResult> Complete(int assignmentId)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/complete?workerId=...");
            await _executionService.CompleteAssemblyTask(assignmentId);
            return Ok();
        }

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
