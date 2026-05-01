using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.Interface;

namespace TaskControl.TaskModule.Presentation;

[ApiController]
[Route("api/workers/{employeeId}/breaks")]
public class WorkerBreaksController : ControllerBase
{
    private readonly IWorkerBreakService _breakService;
    private readonly ILogger<WorkerBreaksController> _logger;

    public WorkerBreaksController(
        IWorkerBreakService breakService,
        ILogger<WorkerBreaksController> logger)
    {
        _breakService = breakService ?? throw new ArgumentNullException(nameof(breakService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Получить текущий статус перерыва сотрудника
    /// </summary>
    /// <param name="employeeId">ID сотрудника</param>
    [HttpGet("status")]
    public async Task<ActionResult<BreakStatusDto>> GetBreakStatus(int employeeId)
    {
        _logger.LogInformation("Запрос на получение статуса перерыва для сотрудника ID: {EmployeeId}", employeeId);
        try
        {
            var status = await _breakService.GetBreakStatusAsync(employeeId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статуса перерыва для сотрудника ID: {EmployeeId}", employeeId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Уйти на перерыв
    /// </summary>
    /// <param name="employeeId">ID сотрудника</param>
    [HttpPost("start")]
    public async Task<IActionResult> StartBreak(int employeeId)
    {
        _logger.LogInformation("Запрос на начало перерыва для сотрудника ID: {EmployeeId}", employeeId);
        try
        {
            await _breakService.StartBreakAsync(employeeId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            // Используем Warning для бизнес-ошибок (например, лимит отдыхающих исчерпан)
            _logger.LogWarning(ex, "Отклонен запрос на старт перерыва для сотрудника ID: {EmployeeId}. Причина: {Reason}", employeeId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Внутренняя ошибка сервера при старте перерыва для сотрудника ID: {EmployeeId}", employeeId);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера при старте перерыва." });
        }
    }

    /// <summary>
    /// Вернуться с перерыва
    /// </summary>
    /// <param name="employeeId">ID сотрудника</param>
    [HttpPost("end")]
    public async Task<IActionResult> EndBreak(int employeeId)
    {
        _logger.LogInformation("Запрос на завершение перерыва для сотрудника ID: {EmployeeId}", employeeId);
        try
        {
            await _breakService.EndBreakAsync(employeeId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Ошибка бизнес-логики при завершении перерыва для сотрудника ID: {EmployeeId}. Причина: {Reason}", employeeId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Внутренняя ошибка сервера при завершении перерыва для сотрудника ID: {EmployeeId}", employeeId);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера при завершении перерыва." });
        }
    }
}