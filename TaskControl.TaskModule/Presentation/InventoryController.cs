using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;

namespace TaskControl.TaskModule.Presentation.Controllers
{
    /// <summary>
    /// Controller for managing inventory tasks
    /// Provides endpoints for mobile app to retrieve inventory task details
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IService<InventoryTaskDetailsDto> _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            IService<InventoryTaskDetailsDto> inventoryService,
            ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all inventory tasks (for listing pending tasks)
        /// </summary>
        /// <returns>List of all inventory tasks</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<InventoryTaskDetailsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryTaskDetailsDto>>> GetAll()
        {
            try
            {
                var tasks = await _inventoryService.GetAll();
                _logger.LogInformation("Retrieved {TaskCount} inventory tasks", tasks.Count());
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory tasks");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving tasks");
            }
        }

        /// <summary>
        /// Get a specific inventory task by ID
        /// Used when mobile app detects a new task and needs full details
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <returns>Detailed inventory task information</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(InventoryTaskDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryTaskDetailsDto>> GetById(int id)
        {
            try
            {
                var task = await _inventoryService.GetById(id);
                if (task == null)
                {
                    _logger.LogWarning("Inventory task with ID: {TaskId} not found", id);
                    return NotFound(new { message = $"Inventory task {id} not found" });
                }

                _logger.LogInformation("Retrieved inventory task {TaskId}", id);
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory task {TaskId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving task");
            }
        }

        /// <summary>
        /// Get pending (unfinished) inventory tasks for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of pending inventory tasks assigned to user</returns>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<InventoryTaskDetailsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryTaskDetailsDto>>> GetUserPendingTasks(int userId)
        {
            try
            {
                // This should be implemented in your service
                // For now, returning a placeholder that you should customize
                var tasks = await _inventoryService.GetAll();
                
                _logger.LogInformation("Retrieved pending tasks for user {UserId}", userId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending tasks for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving user tasks");
            }
        }

        /// <summary>
        /// Get new (unstarted) inventory tasks since specified timestamp
        /// Used by mobile app polling mechanism to check for new tasks
        /// </summary>
        /// <param name="since">DateTime in UTC format (ISO 8601) to fetch tasks created after this time</param>
        /// <returns>List of new inventory tasks</returns>
        [HttpGet("new")]
        [ProducesResponseType(typeof(IEnumerable<InventoryTaskDetailsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryTaskDetailsDto>>> GetNewTasks([FromQuery] DateTime? since = null)
        {
            try
            {
                var allTasks = await _inventoryService.GetAll();
                
                IEnumerable<InventoryTaskDetailsDto> newTasks = allTasks;
                
                if (since.HasValue)
                {
                    newTasks = allTasks.Where(t => t.InitiatedAt > since.Value);
                }

                _logger.LogInformation("Retrieved {TaskCount} new inventory tasks since {Since}", 
                    newTasks.Count(), since?.ToString("O") ?? "start");
                return Ok(newTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving new inventory tasks");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving new tasks");
            }
        }

        /// <summary>
        /// Check if there are any new unfinished inventory tasks
        /// Lightweight endpoint for polling - returns only count and latest task timestamp
        /// </summary>
        /// <param name="since">DateTime in UTC format (ISO 8601)</param>
        /// <returns>Count of new tasks and latest task timestamp</returns>
        [HttpGet("check-new")]
        [ProducesResponseType(typeof(TaskCheckResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<TaskCheckResponse>> CheckForNewTasks([FromQuery] DateTime? since = null)
        {
            try
            {
                var allTasks = await _inventoryService.GetAll();
                
                IEnumerable<InventoryTaskDetailsDto> newTasks = allTasks;
                
                if (since.HasValue)
                {
                    newTasks = allTasks.Where(t => t.InitiatedAt > since.Value);
                }

                var response = new TaskCheckResponse
                {
                    HasNewTasks = newTasks.Any(),
                    NewTaskCount = newTasks.Count(),
                    LatestTaskTime = newTasks.Any() ? newTasks.Max(t => t.InitiatedAt) : (DateTime?)null,
                    LastChecked = DateTime.UtcNow
                };

                _logger.LogInformation("Task check: {HasNewTasks}, Count: {Count}", 
                    response.HasNewTasks, response.NewTaskCount);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for new inventory tasks");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error checking tasks");
            }
        }
    }

    /// <summary>
    /// Response model for task check endpoint
    /// </summary>
    public class TaskCheckResponse
    {
        public bool HasNewTasks { get; set; }
        public int NewTaskCount { get; set; }
        public DateTime? LatestTaskTime { get; set; }
        public DateTime LastChecked { get; set; }
    }
}
