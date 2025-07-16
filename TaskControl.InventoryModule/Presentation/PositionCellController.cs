using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Application.DTOs;

namespace TaskControl.InventoryModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionCellController : ControllerBase, ICrudController<PositionCellDto, int>
    {
        private readonly IService<PositionCellDto> _service;
        private readonly ILogger<PositionCellController> _logger;

        public PositionCellController(
            IService<PositionCellDto> service,
            ILogger<PositionCellController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PositionCellDto>>> GetAll()
        {
            var records = await _service.GetAll();
            _logger.LogInformation("Получено {Count} складских позиций", records.Count());
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PositionCellDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null)
            {
                _logger.LogWarning("Складская позиция с ID: {PositionId} не найдена", id);
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(PositionCellDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = await _service.Add(dto);
            _logger.LogInformation("Добавлена новая складская позиция. ID: {PositionId}, Зона: {Zone}", newId, dto.ZoneCode);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(PositionCellDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.Update(dto);
            if (!result)
            {
                _logger.LogWarning("Попытка обновления несуществующей складской позиции ID: {PositionId}", dto.PositionId);
                return NotFound();
            }
            _logger.LogInformation("Складская позиция ID: {PositionId} обновлена", dto.PositionId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            if (!result)
            {
                _logger.LogWarning("Попытка удаления несуществующей складской позиции ID: {PositionId}", id);
                return NotFound();
            }
            _logger.LogInformation("Складская позиция ID: {PositionId} удалена", id);
            return NoContent();
        }
    }
}