using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interfaces;

namespace TaskControl.TaskModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseTasksController : ControllerBase, ICrudController<BaseTaskDto, int>
    {
        private readonly IService<BaseTaskDto> _service;
        private readonly ILogger<BaseTasksController> _logger;
        private readonly IInventarizationQueryService _inventarizationQueryService;

        public BaseTasksController(
            IService<BaseTaskDto> service,
            IInventarizationQueryService inventarizationQueryService,
            ILogger<BaseTasksController> logger)
        {
            _service = service;
            _inventarizationQueryService = inventarizationQueryService;
            _logger = logger;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<BaseTaskDto>>> GetAll()
        {
            var records = await _service.GetAll();
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseTaskDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null)
            {
                _logger.LogWarning("Задача с ID: {TaskId} не найдена", id);
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(BaseTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = await _service.Add(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(BaseTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.Update(dto);
            if (!result)
            {
                _logger.LogWarning("Попытка обновления несуществующей задачи ID: {TaskId}", dto.TaskId);
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
                _logger.LogWarning("Попытка удаления несуществующей задачи ID: {TaskId}", id);
                return NotFound();
            }
            return NoContent();
        }
        ////////////////////////////////////////////////////////////////////////////Блок Инвентаризации
        /// <summary>
        /// Детали инвентаризации (товары, ожидаемое количество, позиции)
        /// </summary>
        [HttpGet("inventarization/{taskId:int}/details")]
        public async Task<ActionResult<InventoryTaskDetailsDto>> GetInventarizationDetails(int taskId)
        {
            var details = await _inventarizationQueryService.GetInventoryTaskDetailsAsync(taskId);

            if (details == null)
            {
                _logger.LogWarning("Детали инвентаризации не найдены для задачи ID: {TaskId}", taskId);
                return NotFound();
            }

            return Ok(details);
        }
    }
}