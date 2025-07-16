using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Application.DTOs;

namespace TaskControl.InventoryModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemStatusController : ControllerBase, ICrudController<ItemStatusDto, int>
    {
        private readonly IService<ItemStatusDto> _service;
        private readonly ILogger<ItemStatusController> _logger;

        public ItemStatusController(
            IService<ItemStatusDto> service,
            ILogger<ItemStatusController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemStatusDto>>> GetAll()
        {
            var records = await _service.GetAll();
            _logger.LogInformation("Получено {Count} статусов товаров", records.Count());
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemStatusDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null)
            {
                _logger.LogWarning("Статус товара с ID: {StatusId} не найден", id);
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(ItemStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = await _service.Add(dto);
            _logger.LogInformation("Добавлен новый статус товара. ID: {StatusId}, Статус: {Status}", newId, dto.Status);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(ItemStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.Update(dto);
            if (!result)
            {
                _logger.LogWarning("Попытка обновления несуществующего статуса товара ID: {StatusId}", dto.Id);
                return NotFound();
            }
            _logger.LogInformation("Статус товара ID: {StatusId} обновлен", dto.Id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            if (!result)
            {
                _logger.LogWarning("Попытка удаления несуществующего статуса товара ID: {StatusId}", id);
                return NotFound();
            }
            _logger.LogInformation("Статус товара ID: {StatusId} удален", id);
            return NoContent();
        }
    }
}