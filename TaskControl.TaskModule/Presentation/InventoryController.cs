using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.Application.Services;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Presentation.Interface;

namespace TaskControl.TaskModule.Presentation
{
    /// <summary>
    /// REST API контроллер для управления инвентаризацией
    /// Основной интерфейс для клиентских приложений
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryProcessService _processService;
        private readonly IDiscrepancyManagementService _discrepancyService;
        private readonly ActiveEmployeeService _activeEmployeeService;
        private readonly IInventoryReportService _reportService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            IInventoryProcessService processService,
            IDiscrepancyManagementService discrepancyService,
            ActiveEmployeeService activeEmployeeService,
            IInventoryReportService reportService,
            ILogger<InventoryController> logger)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _discrepancyService = discrepancyService ?? throw new ArgumentNullException(nameof(discrepancyService));
            _activeEmployeeService = activeEmployeeService ?? throw new ArgumentNullException(nameof(activeEmployeeService));
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Создать новую задачу инвентаризации и распределить между работникам. Получает список товаров
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// POST /api/v1/inventory/create
        /// {
        ///     "branchId": 1,
        ///     "priority": 8,
        ///     "itemPositionIds": [1, 2, 3, 4, 5],
        ///     "workerCount": 2,
        ///     "description": "Плановая инвентаризация зоны A",
        ///     "divisionStrategy": "ByQuantity",
        ///     "deadlineDate": "2025-12-18T14:00:00Z"
        /// }
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "message": "Инвентаризация создана и распределена между 2 работниками"
        /// }
        /// </remarks>
        [HttpPost("create")]
        [ProducesResponseType(typeof(CompleteInventoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateInventory(
            [FromBody] CreateInventoryTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation(
                    "Запрос на создание инвентаризации: филиал {BranchId}, приоритет {Priority}, позиций {ItemCount}",
                    dto.BranchId, dto.Priority, dto.ItemPositionIds.Count);

                // TODO: Получить доступных работников по филиалу (заменить на метод)
                var availableWorkers = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(dto.BranchId);
                var workerIds = availableWorkers.Select(w => w.EmployeeId).ToList();
                var result = await _processService.CreateAndDistributeInventoryAsync(dto, workerIds);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании инвентаризации");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("worker/{userId}/new-tasks")]
        [ProducesResponseType(typeof(List<InventoryAssignmentDetailedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNewWorkerTasks(int userId)
        {
            try
            {
                _logger.LogInformation("Получение новых задач для работника {UserId}", userId);

                var tasks = await _processService.GetNewAssignmentsForWorkerAsync(userId);
                return Ok(tasks);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении новых задач для работника {UserId}", userId);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("worker/{userId}/tasks/{inventoryTaskId}/details")]
        [ProducesResponseType(typeof(InventoryTaskDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventoryTaskDetails(int userId, int inventoryTaskId)
        {
            try
            {
                _logger.LogInformation(
                    "Получение деталей задачи инвентаризации {TaskId} для работника {UserId}",
                    inventoryTaskId, userId);

                var dto = await _processService.GetInventoryTaskDetailsForWorkerAsync(userId, inventoryTaskId);
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при получении деталей задачи инвентаризации {TaskId} для работника {UserId}",
                    inventoryTaskId, userId);

                return BadRequest(new { error = ex.Message });
            }
        }




        /// <summary>
        /// Обработать сканирование товара (установить фактическое количество)
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// POST /api/v1/inventory/scan
        /// {
        ///     "assignmentId": 1,
        ///     "lineId": 5,
        ///     "actualQuantity": 12,
        ///     "userId": 10,
        ///     "note": "На полке обнаружено 2 дополнительных единицы"
        /// }
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "id": 1,
        ///     "inventoryAssignmentId": 1,
        ///     "totalPositions": 30,
        ///     "countedPositions": 5,
        ///     "completionPercentage": 16.67,
        ///     "discrepancyCount": 1,
        ///     "surplusCount": 1,
        ///     "shortageCount": 0
        /// }
        /// </remarks>
        [HttpPost("scan")]
        [ProducesResponseType(typeof(InventoryStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ProcessScan(
            [FromBody] ProcessInventoryScanDto dto
            )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation(
                    "Сканирование: назначение {AssignmentId}, строка {LineId}, количество {ActualQuantity}",
                    dto.AssignmentId, dto.LineId, dto.ActualQuantity);

                var statistics = await _processService.ProcessInventoryScanAsync(dto);

                return Ok(statistics);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке сканирования");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить текущий прогресс инвентаризации
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/inventory/1/progress
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "assignmentId": 1,
        ///     "currentStatistics": {
        ///         "totalPositions": 30,
        ///         "countedPositions": 15,
        ///         "completionPercentage": 50.0
        ///     },
        ///     "remainingItems": [
        ///         {
        ///             "itemPositionId": 5,
        ///             "expectedQuantity": 10,
        ///             "zoneCode": "A"
        ///         }
        ///     ],
        ///     "status": "InProgress"
        /// }
        /// </remarks>
        [HttpGet("{assignmentId}/progress")]
        [ProducesResponseType(typeof(GetInventoryProgressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProgress(
            int assignmentId)
        {
            try
            {
                _logger.LogInformation("Получение прогресса инвентаризации {AssignmentId}", assignmentId);

                var progress = await _processService.GetInventoryProgressAsync(
                    assignmentId);

                return Ok(progress);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении прогресса");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Завершить инвентаризацию и получить финальный отчёт
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// POST /api/v1/inventory/1/complete
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "assignmentId": 1,
        ///     "statistics": {
        ///         "totalPositions": 30,
        ///         "countedPositions": 30,
        ///         "completionPercentage": 100.0,
        ///         "discrepancyCount": 2
        ///     },
        ///     "discrepancyReport": {
        ///         "totalDiscrepancies": 2,
        ///         "surplusCount": 1,
        ///         "shortageCount": 1
        ///     },
        ///     "completedAt": "2025-12-17T14:30:00Z",
        ///     "message": "Инвентаризация завершена. Найдено 2 расхождений"
        /// }
        /// </remarks>
        [HttpPost("{assignmentId}/complete")]
        [ProducesResponseType(typeof(CompleteInventoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteInventory(
            int assignmentId)
        {
            try
            {
                _logger.LogInformation("Завершение инвентаризации {AssignmentId}", assignmentId);

                var result = await _processService.CompleteInventoryAsync(
                    assignmentId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при завершении инвентаризации");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить список расхождений для инвентаризации
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/inventory/1/discrepancies
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "inventoryAssignmentId": 1,
        ///     "totalDiscrepancies": 2,
        ///     "surplusCount": 1,
        ///     "shortageCount": 1,
        ///     "discrepancies": [
        ///         {
        ///             "id": 1,
        ///             "itemPositionId": 5,
        ///             "expectedQuantity": 10,
        ///             "actualQuantity": 12,
        ///             "variance": 2,
        ///             "type": "Surplus"
        ///         }
        ///     ]
        /// }
        /// </remarks>
        [HttpGet("{assignmentId}/discrepancies")]
        [ProducesResponseType(typeof(DiscrepancyReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDiscrepancies(
            int assignmentId)
        {
            try
            {
                _logger.LogInformation("Получение расхождений для назначения {AssignmentId}", assignmentId);

                var report = await _discrepancyService.GetDiscrepanciesAsync(assignmentId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении расхождений");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить все нерешённые расхождения по системе
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/inventory/discrepancies/pending
        /// 
        /// Ответ 200 OK:
        /// [
        ///     {
        ///         "id": 1,
        ///         "inventoryAssignmentLineId": 5,
        ///         "type": "Surplus",
        ///         "variance": 2,
        ///         "resolutionStatus": "Pending"
        ///     }
        /// ]
        /// </remarks>
        [HttpGet("discrepancies/pending")]
        [ProducesResponseType(typeof(List<DiscrepancyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingDiscrepancies()
        {
            try
            {
                _logger.LogInformation("Получение всех нерешённых расхождений");

                var discrepancies = await _discrepancyService.GetPendingDiscrepanciesAsync();
                return Ok(discrepancies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении нерешённых расхождений");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Разрешить расхождение (отметить как решённое, в расследовании или списано)
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// POST /api/v1/inventory/discrepancy/resolve
        /// {
        ///     "discrepancyId": 1,
        ///     "resolutionStatus": "Resolved",
        ///     "reason": "Товар найден на другой полке"
        /// }
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "id": 1,
        ///     "type": "Surplus",
        ///     "resolutionStatus": "Resolved"
        /// }
        /// </remarks>
        [HttpPost("discrepancy/resolve")]
        [ProducesResponseType(typeof(DiscrepancyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResolveDiscrepancy(
            [FromBody] ResolveDiscrepancyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation(
                    "Разрешение расхождения {DiscrepancyId}, статус {Status}",
                    dto.DiscrepancyId, dto.ResolutionStatus);

                var result = await _discrepancyService.ResolveDiscrepancyAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при разрешении расхождения");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить статистику инвентаризации
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/inventory/1/statistics
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "id": 1,
        ///     "inventoryAssignmentId": 1,
        ///     "totalPositions": 30,
        ///     "countedPositions": 30,
        ///     "completionPercentage": 100.0,
        ///     "discrepancyPercentage": 6.67,
        ///     "durationSeconds": 14400
        /// }
        /// </remarks>
        [HttpGet("{assignmentId}/statistics")]
        [ProducesResponseType(typeof(InventoryStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatistics(
            int assignmentId)
        {
            try
            {
                _logger.LogInformation("Получение статистики инвентаризации {AssignmentId}", assignmentId);

                var statistics = await _reportService.GetInventoryStatisticsAsync(assignmentId);
                return Ok(statistics);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить отчёт о производительности работника
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/inventory/worker/10/performance?from=2025-12-01&to=2025-12-31
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "userId": 10,
        ///     "periodFrom": "2025-12-01T00:00:00Z",
        ///     "periodTo": "2025-12-31T23:59:59Z",
        ///     "completedInventories": 5,
        ///     "totalItemsCount": 150,
        ///     "averageAccuracy": 98.5,
        ///     "averageDurationSeconds": 3600
        /// }
        /// </remarks>
        [HttpGet("worker/{userId}/performance")]
        [ProducesResponseType(typeof(WorkerPerformanceReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWorkerPerformance(
            int userId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                _logger.LogInformation(
                    "Получение отчёта производительности для работника {UserId} с {From} по {To}",
                    userId, from, to);

                var report = await _reportService.GetWorkerPerformanceAsync(userId, from, to);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении отчёта производительности");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("check-new")]
        [ProducesResponseType(typeof(TaskCheckResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckForNewTasks([FromQuery] DateTime? since = null)
        {
            try
            {
                var allTasks = await _processService.GetActiveInventoriesAsync();

                IEnumerable<InventoryAssignmentDetailedDto> newTasks = allTasks;

                if (since.HasValue)
                    newTasks = allTasks.Where(t => t.AssignedAt > since.Value);

                var response = new TaskCheckResponse
                {
                    HasNewTasks = newTasks.Any(),
                    NewTaskCount = newTasks.Count(),
                    LatestTaskTime = newTasks.Any() ? newTasks.Max(t => t.AssignedAt) : (DateTime?)null,
                    LastChecked = DateTime.UtcNow
                };

                _logger.LogInformation("Есть новые задачи: {HasNewTasks}, Количество: {Count}", response.HasNewTasks, response.NewTaskCount);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка во время проверки наличия новых задач");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ошибка проверки наличия новых задач");
            }
        }



    }

    public class TaskCheckResponse
    {
        public bool HasNewTasks { get; set; }
        public int NewTaskCount { get; set; }
        public DateTime? LatestTaskTime { get; set; }
        public DateTime LastChecked { get; set; }
    }
}
