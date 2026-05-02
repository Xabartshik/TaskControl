using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;

namespace TaskControl.TaskModule.Presentation
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryProcessService _processService;

        public InventoryController(IInventoryProcessService processService)
        {
            _processService = processService;
        }

        private void AddDeprecationWarning(string replacement)
        {
            Response.Headers.Append("Warning", $"299 - Deprecated API. Use {replacement}");
            Response.Headers.Append("X-Api-Deprecated", "true");
        }

        [HttpPost("create-distributed")]
        public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryTaskDto request)
        {
            var result = await _processService.CreateAndDistributeInventoryAsync(request, request.WorkerIds ?? new List<int>());
            return Ok(result);
        }

        /// <summary>DEPRECATED: используйте GET /api/v1/WorkerTasks/{workerId}/pending.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{workerId}/pending")]
        [HttpGet("worker/{userId}/assignments")]
        public async Task<IActionResult> GetHeaders(int userId)
        {
            AddDeprecationWarning("GET /api/v1/WorkerTasks/{workerId}/pending");
            return Ok(await _processService.GetAssignmentsHeaderForWorkerAsync(userId));
        }

        /// <summary>DEPRECATED: используйте GET /api/v1/WorkerTasks/{taskId}/details.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/details")]
        [HttpGet("assignment/{id}/details")]
        public async Task<IActionResult> GetDetails(int id)
        {
            AddDeprecationWarning("GET /api/v1/WorkerTasks/{taskId}/details");
            return Ok(await _processService.GetInventoryTaskDetailsAsync(id));
        }

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/start?workerId=...</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/start")]
        [HttpPost("assignment/{id}/start")]
        public async Task<IActionResult> Start(int id)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/start?workerId=...");
            return Ok(await _processService.StartInventoryAsync(id));
        }

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/pause.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/pause")]
        [HttpPost("assignment/{id}/pause")]
        public async Task<IActionResult> Pause(int id)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/pause");
            return Ok(await _processService.PauseInventoryAsync(id));
        }

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/cancel.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/cancel")]
        [HttpPost("assignment/{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/cancel");
            return Ok(await _processService.CancelInventoryAsync(id));
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ProcessScan([FromBody] ProcessInventoryScanDto dto)
            => Ok(await _processService.ProcessScanAsync(dto));

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/complete?workerId=...</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/complete")]
        [HttpPost("complete-assignment")]
        public async Task<IActionResult> Complete([FromBody] CompleteAssignmentDto dto)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/complete?workerId=...");
            return Ok(await _processService.CompleteAssignmentAsync(dto));
        }
    }
}
