using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;
using TaskControl.TaskModule.Application.Interface;

namespace TaskControl.TaskModule.Presentation
{
    /// <summary>
    /// REST API контроллер для панели начальника
    /// Основной интерфейс для управления задачами своего филиала
    /// </summary>
    [ApiController]
    [Route("api/v1/boss-panel/{bossBranchId}")]
    [Produces("application/json")]
    public class BossPanelController : ControllerBase
    {
        private readonly IBossPanelService _bossPanelService;
        private readonly ILogger<BossPanelController> _logger;

        public BossPanelController(
            IBossPanelService bossPanelService,
            ILogger<BossPanelController> logger)
        {
            _bossPanelService = bossPanelService ?? throw new ArgumentNullException(nameof(bossPanelService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Создать новую задачу инвентаризации для филиала начальника
        /// </summary>
        [HttpPost("inventory/create")]
        [ProducesResponseType(typeof(CompleteInventoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateInventory(
            int bossBranchId,
            [FromBody] CreateInventoryTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation(
                    "Начальник филиала {BossBranchId} запрашивает создание инвентаризации", bossBranchId);

                var result = await _bossPanelService.CreateInventoryTaskAsync(dto, bossBranchId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Ошибка валидации при создании инвентаризации начальником филиала {BossBranchId}", bossBranchId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании инвентаризации начальником филиала {BossBranchId}", bossBranchId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить отчет по сотрудникам филиала начальника
        /// </summary>
        [HttpGet("workers/performance")]
        [ProducesResponseType(typeof(IEnumerable<WorkerPerformanceReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBranchWorkersPerformance(
            int bossBranchId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                _logger.LogInformation(
                    "Начальник филиала {BossBranchId} запрашивает отчеты сотрудников с {From} по {To}", bossBranchId, from, to);

                var report = await _bossPanelService.GetBranchWorkersPerformanceAsync(bossBranchId, from, to);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении отчета производительности начальником филиала {BossBranchId}", bossBranchId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить расхождения инвентаризации, если она относится к филиалу начальника
        /// </summary>
        [HttpGet("inventory/{assignmentId}/discrepancies")]
        [ProducesResponseType(typeof(DiscrepancyReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBranchInventoryDiscrepancies(
            int bossBranchId,
            int assignmentId)
        {
            try
            {
                _logger.LogInformation("Начальник филиала {BossBranchId} запрашивает расхождения для назначения {AssignmentId}", bossBranchId, assignmentId);

                var report = await _bossPanelService.GetBranchInventoryDiscrepanciesAsync(bossBranchId, assignmentId);
                return Ok(report);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Ошибка валидации при получении расхождений инвентаризации {AssignmentId} начальником филиала {BossBranchId}", assignmentId, bossBranchId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении расхождений инвентаризации {AssignmentId} начальником филиала {BossBranchId}", assignmentId, bossBranchId);
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
