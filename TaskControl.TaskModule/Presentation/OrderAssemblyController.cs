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

        [HttpGet("worker/{userId}/assignments")]
        public async Task<IActionResult> GetHeaders(int userId)
        {
            return Ok(await _executionService.GetAssignmentsHeaderForWorkerAsync(userId));
        }

        [HttpGet("assignment/{id}/details")]
        public async Task<IActionResult> GetDetails(int id)
        {
            try
            {
                return Ok(await _executionService.GetAssemblyTaskDetailsAsync(id));
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost("assignment/{id}/start")]
        public async Task<IActionResult> Start(int id)
        {
            var result = await _executionService.StartAssemblyAsync(id);
            return result ? Ok() : NotFound();
        }

        [HttpPost("assignment/{id}/pause")]
        public async Task<IActionResult> Pause(int id)
        {
            var result = await _executionService.PauseAssemblyAsync(id);
            return result ? Ok() : NotFound();
        }

        [HttpPost("assignment/{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _executionService.CancelAssemblyAsync(id);
            return result ? Ok() : NotFound();
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

        [HttpPost("complete/{assignmentId}")]
        public async Task<IActionResult> CompleteTask(int assignmentId)
        {
            await _executionService.CompleteAssemblyTask(assignmentId);
            return Ok();
        }
    }

    public class ScanPickRequest { public int LineId { get; set; } public string Barcode { get; set; } }
    public class ScanPlaceBulkRequest { public int AssignmentId { get; set; } public string CellCode { get; set; } }
    public class ReportMissingRequest { public int LineId { get; set; } public string Reason { get; set; } }
}