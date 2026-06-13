using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.OrderModule.Application.Services;

namespace TaskControl.InventoryModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemPositionController : ControllerBase, ICrudController<ItemPositionDto, int>
    {
        private readonly ItemPositionService _service;
        private readonly ILogger<ItemPositionController> _logger;

        public ItemPositionController(
            ItemPositionService service,
            ILogger<ItemPositionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemPositionDto>>> GetAll()
        {
            var records = await _service.GetAll();
            _logger.LogInformation("Получено {Count} товарных позиций", records.Count());
            return Ok(records);
        }

        [HttpGet("available/{branchId}")]
        public async Task<ActionResult<IEnumerable<AvailableItemDto>>> GetAvailableItems(int branchId, [FromQuery] string? search)
        {
            // Передаем параметр search в сервис
            var items = await _service.GetAvailableItemsByBranchAsync(branchId, search);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemPositionDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null)
            {
                _logger.LogWarning("Товарная позиция с ID: {PositionId} не найдена", id);
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(ItemPositionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = await _service.Add(dto);
            _logger.LogInformation("Добавлена новая товарная позиция. ID: {PositionId}, Товар: {ItemId}", newId, dto.ItemId);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(ItemPositionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.Update(dto);
            if (!result)
            {
                _logger.LogWarning("Попытка обновления несуществующей товарной позиции ID: {PositionId}", dto.Id);
                return NotFound();
            }
            _logger.LogInformation("Товарная позиция ID: {PositionId} обновлена", dto.Id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            if (!result)
            {
                _logger.LogWarning("Попытка удаления несуществующей товарной позиции ID: {PositionId}", id);
                return NotFound();
            }
            _logger.LogInformation("Товарная позиция ID: {PositionId} удалена", id);
            return NoContent();
        }

        /// <summary>
        /// Возвращает доступность товаров корзины по всем филиалам.
        /// </summary>
        [HttpPost("check-cart-branches")]
        public async Task<ActionResult<BranchAvailabilityResponseDto>> CheckCartBranches([FromBody] CartCheckRequestDto request)
        {
            if (request == null || request.Items == null)
            {
                return BadRequest("Неверный запрос.");
            }

            var availability = await _service.CheckCartAvailabilityAsync(request.Items);
            return Ok(availability);
        }

        [HttpGet("branch-counts")]
        public async Task<ActionResult<Dictionary<int, ItemStockDto>>> GetItemBranchCounts()
        {
            var counts = await _service.GetItemBranchCountsAsync();
            return Ok(counts);
        }

        /// <summary>
        /// Возвращает распределение остатков конкретного товара по всем филиалам.
        /// </summary>
        [HttpGet("item-stock-distribution/{itemId}")]
        public async Task<ActionResult<IEnumerable<BranchStockDto>>> GetItemStockDistribution(int itemId)
        {
            var distribution = await _service.GetItemStockDistributionAsync(itemId);
            return Ok(distribution);
        }
    }
}