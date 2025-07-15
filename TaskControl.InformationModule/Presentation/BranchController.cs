using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.Services;

namespace TaskControl.InformationModule.Presentation.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class BranchesController : ControllerBase
    {
        private readonly BranchService _branchService;
        private readonly ILogger<BranchesController> _logger;

        public BranchesController(
            BranchService branchService,
            ILogger<BranchesController> logger)
        {
            _branchService = branchService;
            _logger = logger;
        }

        /// <summary>
        /// Получить все филиалы
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BranchDto>>> GetAll()
        {
            var branches = await _branchService.GetAll();
            return Ok(branches);
        }

        /// <summary>
        /// Получить филиал по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BranchDto>> GetById(int id)
        {
            var branch = await _branchService.GetById(id);

            if (branch == null)
            {
                _logger.LogWarning("Филиал с ID: {BranchId} не найден", id);
                return NotFound();
            }

            return Ok(branch);
        }

        /// <summary>
        /// Добавить новый филиал
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<int>> Create(BranchDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = await _branchService.Add(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        /// <summary>
        /// Обновить филиал
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BranchDto dto)
        {
            if (id != dto.BranchId)
            {
                ModelState.AddModelError("BranchId", "ID в пути не соответствует ID в теле");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _branchService.Update(dto);

            if (!result)
            {
                _logger.LogWarning("Попытка обновления несуществующего филиала ID: {BranchId}", id);
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Удалить филиал
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _branchService.Delete(id);

            if (!result)
            {
                _logger.LogWarning("Попытка удаления несуществующего филиала ID: {BranchId}", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}