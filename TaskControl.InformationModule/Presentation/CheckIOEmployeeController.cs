using Microsoft.AspNetCore.Mvc;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InformationModule.Application.DTOs;

namespace TaskControl.InformationModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckIOEmployeeController : ControllerBase
    {
        private readonly IService<CheckIOEmployeeDto> _service;

        public CheckIOEmployeeController(IService<CheckIOEmployeeDto> service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CheckIOEmployeeDto>>> GetAll()
        {
            var records = await _service.GetAll();
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CheckIOEmployeeDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null)
            {
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(CheckIOEmployeeDto dto)
        {
            var newId = await _service.Add(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(CheckIOEmployeeDto dto)
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