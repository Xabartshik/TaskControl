using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.DTOs.BossPanelDTOs;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.InventoryModule.Application.DTOs;

namespace TaskControl.TaskModule.Presentation
{
    /// <summary>
    /// REST API контроллер для панели начальника
    /// Основной интерфейс для управления задачами своего филиала
    /// </summary>
    [ApiController]
    [Route("api/v1/bosspanel")]
    [Produces("application/json")]
    [Authorize(Roles = "Supervisor,Admin")]
    public class BossPanelController : ControllerBase
    {
        private readonly IBossPanelService _bossPanelService;
        private readonly IBaseTaskService _baseTaskService;
        private readonly ILogger<BossPanelController> _logger;

        public BossPanelController(
            IBossPanelService bossPanelService,
            IBaseTaskService baseTaskService,
            ILogger<BossPanelController> logger)
        {
            _bossPanelService = bossPanelService ?? throw new ArgumentNullException(nameof(bossPanelService));
            _baseTaskService = baseTaskService ?? throw new ArgumentNullException(nameof(baseTaskService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private int? GetBranchIdFromToken()
        {
            var claim = User.FindFirst("BranchId")?.Value;
            if (int.TryParse(claim, out int branchId))
                return branchId;
            return null;
        }

        /// <summary>
        /// Получить список активных задач для текущего филиала
        /// </summary>
        [HttpGet("tasks/active")]
        [ProducesResponseType(typeof(IEnumerable<BossPanelTaskCardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveTasks()
        {
            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                var tasks = await _bossPanelService.GetActiveTasksAsync(branchId.Value);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении активных задач");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить загруженность сотрудников для текущего филиала
        /// </summary>
        [HttpGet("employees/workload")]
        [ProducesResponseType(typeof(IEnumerable<EmployeeWorkloadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmployeeWorkload()
        {
            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                var workload = await _bossPanelService.GetEmployeeWorkloadAsync(branchId.Value);
                return Ok(workload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении загруженности сотрудников");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить доступных сотрудников (в т.ч. рекомендованных)
        /// </summary>
        [HttpGet("employees/available")]
        [ProducesResponseType(typeof(IEnumerable<AvailableEmployeeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableEmployees()
        {
            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                var available = await _bossPanelService.GetAvailableEmployeesAsync(branchId.Value);
                return Ok(available);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении доступных сотрудников");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Автоматический подбор сотрудников на основе нагрузки
        /// </summary>
        [HttpGet("employees/auto-select")]
        [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAutoSelectedEmployees([FromQuery] int count = 1)
        {
            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            if (count <= 0) return BadRequest(new { message = "Количество должно быть больше 0" });

            try
            {
                var selectedIds = await _baseTaskService.GetAutoSelectedEmployeesAsync(branchId.Value, count);
                return Ok(selectedIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при автоматическом подборе сотрудников");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Создать новую задачу инвентаризации для филиала начальника
        /// </summary>
        [HttpPost("tasks/inventory/create")]
        [ProducesResponseType(typeof(CompleteInventoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                // Принудительно ставим BranchId из токена для безопасности
                dto.BranchId = branchId.Value;

                _logger.LogInformation("Начальник филиала {BossBranchId} запрашивает создание инвентаризации", branchId.Value);
                var result = await _bossPanelService.CreateInventoryTaskAsync(dto, branchId.Value);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Ошибка валидации при создании инвентаризации");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании инвентаризации");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить отчет по сотрудникам филиала начальника
        /// </summary>
        [HttpGet("workers/performance")]
        [ProducesResponseType(typeof(IEnumerable<WorkerPerformanceReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBranchWorkersPerformance(DateTime from, DateTime to)
        {
            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                var report = await _bossPanelService.GetBranchWorkersPerformanceAsync(branchId.Value, from, to);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении отчета производительности");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить расхождения инвентаризации, если она относится к филиалу начальника
        /// </summary>
        [HttpGet("inventory/{assignmentId}/discrepancies")]
        [ProducesResponseType(typeof(DiscrepancyReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBranchInventoryDiscrepancies(int assignmentId)
        {
            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                var report = await _bossPanelService.GetBranchInventoryDiscrepanciesAsync(branchId.Value, assignmentId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении расхождений инвентаризации");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить список доступных зон для филиала
        /// </summary>
        [HttpGet("zones")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableZones()
        {
            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                var zones = await _bossPanelService.GetAvailableZonesAsync(branchId.Value);
                return Ok(zones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить все позиции филиала для древовидного селектора
        /// </summary>
        [HttpGet("positions")]
        [ProducesResponseType(typeof(IEnumerable<PositionCellDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPositions()
        {
            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                var positions = await _bossPanelService.GetPositionsAsync(branchId.Value);
                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении позиций");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Создать задачу инвентаризации на основе выбранных зон
        /// </summary>
        [HttpPost("inventory/create-by-zone")]
        [ProducesResponseType(typeof(CompleteInventoryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateInventoryByZone([FromBody] CreateInventoryByZoneDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                // Логируем попытку для отладки
                _logger.LogInformation("Начальник филиала {BranchId} создает инвентаризацию по зонам: {Zones}",
                    branchId.Value, string.Join(", ", dto.ZonePrefixes));

                var result = await _bossPanelService.CreateInventoryByZoneAsync(dto, branchId.Value);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Ошибки бизнес-логики (например, "зона пуста") отдаем как 400 Bad Request
                _logger.LogWarning(ex, "Ошибка бизнес-логики при создании инвентаризации по зоне");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Реальные падения сервера (например, БД недоступна или NullReference) логируем и отдаем как 500
                _logger.LogError(ex, "Внутренняя ошибка сервера при создании инвентаризации по зоне");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера", details = ex.Message });
            }
        }

        /// <summary>
        /// Получить текущий статус работников (кто на работе, количество активных задач)
        /// </summary>
        [HttpGet("workers/status")]
        [ProducesResponseType(typeof(IEnumerable<WorkerStatusDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveWorkersStatus()
        {
            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                var status = await _bossPanelService.GetActiveWorkersStatusAsync(branchId.Value);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить отчеты, сгруппированные по задачам
        /// </summary>
        [HttpGet("reports/grouped")]
        [ProducesResponseType(typeof(IEnumerable<TaskReportGroupDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGroupedTaskReports()
        {
            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                var reports = await _bossPanelService.GetGroupedTaskReportsAsync(branchId.Value);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить список заказов, доступных для сборки
        /// </summary>
        [HttpGet("orders/available")]
        [ProducesResponseType(typeof(IEnumerable<AvailableOrderDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableOrders()
        {
            var branchId = GetBranchIdFromToken();
            if (!branchId.HasValue) return Unauthorized(new { message = "Отсутствует BranchId в токене" });

            try
            {
                var orders = await _bossPanelService.GetAvailableOrdersAsync(branchId.Value);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении доступных заказов");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
