using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.InventoryModule.Application.Services;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostamatController : ControllerBase
    {
        private readonly IPostamatRepository _postamatRepository;
        private readonly PostamatAllocationService _allocationService;
        private readonly ILogger<PostamatController> _logger;

        public PostamatController(
            IPostamatRepository postamatRepository,
            PostamatAllocationService allocationService,
            ILogger<PostamatController> logger)
        {
            _postamatRepository = postamatRepository;
            _allocationService = allocationService;
            _logger = logger;
        }

        /// <summary>
        /// Возвращает список всех активных терминалов для выбора адреса
        /// GET /api/Postamat
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Postamat>>> GetActivePostamats()
        {
            _logger.LogInformation("Запрошен список активных постаматов.");
            var postamats = await _postamatRepository.GetActivePostamatsAsync();
            return Ok(postamats);
        }

        /// <summary>
        /// Проверка вместимости заказа в выбранный постамат
        /// POST /api/Postamat/check-capacity
        /// </summary>
        [HttpPost("check-capacity")]
        public async Task<ActionResult<bool>> CheckCapacity([FromBody] CheckCapacityRequestDto request)
        {
            if (request == null || !request.ItemsToPack.Any())
            {
                return BadRequest("Некорректный запрос или пустой список товаров.");
            }

            _logger.LogInformation("Проверка вместимости для постамата {PostamatId}", request.PostamatId);

            var hasCapacity = await _allocationService.CheckCapacityAsync(request.PostamatId, request.ItemsToPack);

            return Ok(hasCapacity); // Возвращает true или false
        }
    }
}