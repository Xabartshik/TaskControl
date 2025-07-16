using Microsoft.AspNetCore.Mvc;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.Core.Shared.SharedInterfaces;

namespace TaskControl.InformationModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase, ICrudController<ItemDto, int>
    {
        private readonly IService<ItemDto> _service;

        public ItemController(IService<ItemDto> service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll()
        {
            var items = await _service.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetById(int id)
        {
            var item = await _service.GetById(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(ItemDto dto)
        {
            var newId = await _service.Add(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(ItemDto dto)
        {
            var result = await _service.Update(dto);
            if (!result)
            {
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
                return NotFound();
            }
            return NoContent();
        }
    }
}