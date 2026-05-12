using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public ItemMovementController(IService<ItemMovementDto> service, ILogger<ItemMovementController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemMovementDto>>> GetAll()
        {
            var records = await _service.GetAll();
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemMovementDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null) return NotFound();
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(ItemMovementDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _service.Add(dto);
            _logger.LogInformation("Создано перемещение {Id} для товара {ItemId}", id, dto.ItemId);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut]
        public async Task<IActionResult> Update(ItemMovementDto dto)
        {
            var result = await _service.Update(dto);
            return result ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            return result ? NoContent() : NotFound();
        }
    }
}