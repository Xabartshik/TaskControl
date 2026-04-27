using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("create-distributed")]
        public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryTaskDto request)
        {
            // Передаем список WorkerIds, если он есть в теле запроса, иначе пустой список для автоподбора
            var result = await _processService.CreateAndDistributeInventoryAsync(request, request.WorkerIds ?? new List<int>());
            return Ok(result);
        }

        [HttpGet("worker/{userId}/assignments")]
        public async Task<IActionResult> GetHeaders(int userId)
            => Ok(await _processService.GetAssignmentsHeaderForWorkerAsync(userId));

        [HttpGet("assignment/{id}/details")]
        public async Task<IActionResult> GetDetails(int id)
            => Ok(await _processService.GetInventoryTaskDetailsAsync(id));

        [HttpPost("assignment/{id}/start")]
        public async Task<IActionResult> Start(int id)
            => Ok(await _processService.StartInventoryAsync(id));

        [HttpPost("assignment/{id}/pause")]
        public async Task<IActionResult> Pause(int id)
            => Ok(await _processService.PauseInventoryAsync(id));

        [HttpPost("assignment/{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
            => Ok(await _processService.CancelInventoryAsync(id));

        [HttpPost("scan")]
        public async Task<IActionResult> ProcessScan([FromBody] ProcessInventoryScanDto dto)
            => Ok(await _processService.ProcessScanAsync(dto));

        [HttpPost("complete-assignment")]
        public async Task<IActionResult> Complete([FromBody] CompleteAssignmentDto dto)
            => Ok(await _processService.CompleteAssignmentAsync(dto));
    }
}
