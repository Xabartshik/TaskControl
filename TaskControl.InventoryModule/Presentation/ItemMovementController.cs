using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Application.DTOs;

namespace TaskControl.InventoryModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemMovementController : ControllerBase, ICrudController<ItemMovementDto, int>
    {
        private readonly IService<ItemMovementDto> _service;
        private readonly ILogger<ItemMovementController> _logger;

        public ItemMovementController(
            IService<ItemMovementDto> service,
            ILogger<ItemMovementController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemMovementDto>>> GetAll()
        {
            var records = await _service.GetAll();
            _logger.LogInformation("Получено {Count} записей о перемещениях", records.Count());
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemMovementDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null)
            {
                _logger.LogWarning("Перемещение с ID: {MovementId} не найдено", id);
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(ItemMovementDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = await _service.Add(dto);
            _logger.LogInformation("Добавлено новое перемещение. ID: {MovementId}, Количество: {Quantity}", newId, dto.Quantity);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(ItemMovementDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.Update(dto);
            if (!result)
            {
                _logger.LogWarning("Попытка обновления несуществующего перемещения ID: {MovementId}", dto.Id);
                return NotFound();
            }
            _logger.LogInformation("Перемещение ID: {MovementId} обновлено", dto.Id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            if (!result)
            {
                _logger.LogWarning("Попытка удаления несуществующего перемещения ID: {MovementId}", id);
                return NotFound();
            }
            _logger.LogInformation("Перемещение ID: {MovementId} удалено", id);
            return NoContent();
        }
    }
}