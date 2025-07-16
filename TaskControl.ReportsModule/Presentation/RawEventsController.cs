using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.ReportsModule.Application.DTOs;

namespace TaskControl.ReportsModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RawEventsController : ControllerBase, ICrudController<RawEventDto, int>
    {
        private readonly IService<RawEventDto> _service;
        private readonly ILogger<RawEventsController> _logger;

        public RawEventsController(
            IService<RawEventDto> service,
            ILogger<RawEventsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RawEventDto>>> GetAll()
        {
            var records = await _service.GetAll();
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RawEventDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null)
            {
                _logger.LogWarning("Сырое событие с ID: {ReportId} не найдено", id);
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(RawEventDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = await _service.Add(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(RawEventDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.Update(dto);
            if (!result)
            {
                _logger.LogWarning("Попытка обновления несуществующего сырого события ID: {ReportId}", dto.ReportId);
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
                _logger.LogWarning("Попытка удаления несуществующего сырого события ID: {ReportId}", id);
                return NotFound();
            }
            return NoContent();
        }
    }
}