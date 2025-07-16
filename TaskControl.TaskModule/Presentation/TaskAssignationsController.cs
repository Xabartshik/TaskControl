using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.Services;

namespace TaskControl.TaskModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskAssignationsController : ControllerBase, ICrudController<TaskAssignationDto, int>
    {
        private readonly IService<TaskAssignationDto> _service;
        private readonly ILogger<TaskAssignationsController> _logger;

        public TaskAssignationsController(
            IService<TaskAssignationDto> service,
            ILogger<TaskAssignationsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskAssignationDto>>> GetAll()
        {
            var records = await _service.GetAll();
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskAssignationDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null)
            {
                _logger.LogWarning("Назначение с ID: {AssignationId} не найдено", id);
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(TaskAssignationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = await _service.Add(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(TaskAssignationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.Update(dto);
            if (!result)
            {
                _logger.LogWarning("Попытка обновления несуществующего назначения ID: {AssignationId}", dto.Id);
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            if (!result)
            {
                _logger.LogWarning("Попытка удаления несуществующего назначения ID: {AssignationId}", id);
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskAssignationDto>>> GetByTaskId(int taskId)
        {
            var service = _service as TaskAssignationService;
            if (service == null)
            {
                return BadRequest("Invalid service type");
            }

            var records = await service.GetByTaskId(taskId);
            return Ok(records);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TaskAssignationDto>>> GetByUserId(int userId)
        {
            var service = _service as TaskAssignationService;
            if (service == null)
            {
                return BadRequest("Invalid service type");
            }

            var records = await service.GetByUserId(userId);
            return Ok(records);
        }
    }
}