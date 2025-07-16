using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.OrderModule.Application.DTOs;

namespace TaskControl.OrderModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderPositionController : ControllerBase, ICrudController<OrderPositionDto, int>
    {
        private readonly IService<OrderPositionDto> _service;
        private readonly ILogger<OrderPositionController> _logger;

        public OrderPositionController(
            IService<OrderPositionDto> service,
            ILogger<OrderPositionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderPositionDto>>> GetAll()
        {
            var records = await _service.GetAll();
            _logger.LogInformation("Получено {Count} позиций заказов", records.Count());
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderPositionDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null)
            {
                _logger.LogWarning("Позиция заказа с ID: {PositionId} не найдена", id);
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(OrderPositionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = await _service.Add(dto);
            _logger.LogInformation("Добавлена новая позиция заказа. ID: {PositionId}, Заказ: {OrderId}", newId, dto.OrderId);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(OrderPositionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.Update(dto);
            if (!result)
            {
                _logger.LogWarning("Попытка обновления несуществующей позиции заказа ID: {PositionId}", dto.UniqueId);
                return NotFound();
            }
            _logger.LogInformation("Позиция заказа ID: {PositionId} обновлена", dto.UniqueId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            if (!result)
            {
                _logger.LogWarning("Попытка удаления несуществующей позиции заказа ID: {PositionId}", id);
                return NotFound();
            }
            _logger.LogInformation("Позиция заказа ID: {PositionId} удалена", id);
            return NoContent();
        }
    }
}