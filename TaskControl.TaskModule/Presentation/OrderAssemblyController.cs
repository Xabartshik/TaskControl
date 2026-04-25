using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.TaskModule.Application.Interface;

namespace TaskControl.TaskModule.Presentation
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderAssemblyController : ControllerBase
    {
        private readonly IOrderAssemblyExecutionService _executionService;
        private readonly ILogger<OrderAssemblyController> _logger;
        private readonly AppSettings _appSettings;

        public OrderAssemblyController(
            IOrderAssemblyExecutionService executionService,
            ILogger<OrderAssemblyController> logger,
            IOptions<AppSettings> appSettings)
        {
            _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSettings = appSettings?.Value ?? new AppSettings();
        }

        ///// <summary>
        ///// Получить список активных задач сборки для работника.
        ///// В составе каждой задачи возвращается список целевых ячеек PICKUP с товарами.
        ///// </summary>
        //[HttpGet("tasks/{userId}")]
        //public async Task<IActionResult> GetWorkerTasks(int userId)
        //{
        //    if (_appSettings.EnableDetailedLogging)
        //        _logger.LogTrace("GET /api/OrderAssembly/tasks/{UserId}", userId);

        //    try
        //    {
        //        var tasks = await _executionService.GetWorkerAssemblyTasks(userId);
        //        return Ok(tasks);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Ошибка получения задач сборки");
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}

        /// <summary>
        /// Отсканировать штрих-код товара и подтвердить, что кладовщик забрал его с полки.
        /// </summary>
        [HttpPost("scan-pick")]
        public async Task<IActionResult> ScanAndPick([FromBody] ScanPickRequest req)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("POST /api/OrderAssembly/scan-pick");

            try
            {
                await _executionService.ScanAndPickItem(req.LineId, req.Barcode);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ScanAndPick");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Массовое размещение товаров в ячейку PICKUP.
        /// Кладовщик сканирует код ячейки, и все собранные (Picked) товары,
        /// предназначенные для этой ячейки, автоматически переводятся в статус Placed.
        /// Возвращает количество размещённых товаров и число оставшихся ячеек.
        /// </summary>
        [HttpPost("scan-place-bulk")]
        public async Task<IActionResult> ScanAndPlaceBulk([FromBody] ScanPlaceBulkRequest req)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("POST /api/OrderAssembly/scan-place-bulk");

            try
            {
                var result = await _executionService.ScanAndPlaceBulk(req.AssignmentId, req.CellCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ScanAndPlaceBulk");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Сообщить о недостаче конкретного товара.
        /// </summary>
        [HttpPost("report-missing")]
        public async Task<IActionResult> ReportMissing([FromBody] ReportMissingRequest req)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("POST /api/OrderAssembly/report-missing");

            try
            {
                await _executionService.ReportMissingItem(req.LineId, req.Reason);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ReportMissing");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Завершить задачу сборки (после размещения всех товаров по ячейкам PICKUP).
        /// </summary>
        [HttpPost("complete/{assignmentId}")]
        public async Task<IActionResult> CompleteTask(int assignmentId)
        {
            if (_appSettings.EnableDetailedLogging)
                _logger.LogTrace("POST /api/OrderAssembly/complete/{AssignmentId}", assignmentId);

            try
            {
                await _executionService.CompleteAssemblyTask(assignmentId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при завершении задачи");
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class ScanPickRequest
    {
        public int LineId { get; set; }
        public string Barcode { get; set; }
    }

    public class ScanPlaceBulkRequest
    {
        public int AssignmentId { get; set; }
        public string CellCode { get; set; }
    }

    public class ReportMissingRequest
    {
        public int LineId { get; set; }
        public string Reason { get; set; }
    }
}
