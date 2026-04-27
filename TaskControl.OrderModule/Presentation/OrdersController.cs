using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.OrderModule.Application.DTOs;

namespace TaskControl.OrderModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase, ICrudController<OrderDto, int>
    {
        private readonly IService<OrderDto> _service;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IService<OrderDto> service,
            ILogger<OrdersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
        {
            var records = await _service.GetAll();
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null)
            {
                _logger.LogWarning("Заказ с ID: {OrderId} не найден", id);
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add([FromBody] OrderDto dto)
        {
            try
            {
                var newId = await _service.Add(dto);
                return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Возвращаем 409 Conflict, если невозможно выполнить упаковку в постамат
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании заказа");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(OrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.Update(dto);
            if (!result)
            {
                _logger.LogWarning("Попытка обновления несуществующего заказа ID: {OrderId}", dto.OrderId);
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
                _logger.LogWarning("Попытка удаления несуществующего заказа ID: {OrderId}", id);
                return NotFound();
            }
            return NoContent();
        }
    }
}