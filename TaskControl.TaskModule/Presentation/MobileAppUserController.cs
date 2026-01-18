using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;

namespace TaskControl.TaskModule.Presentation
{
    /// <summary>
    /// REST API контроллер для управления пользователями мобильного приложения
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class MobileAppUserController : ControllerBase
    {
        private readonly IMobileAppUserService _service;
        private readonly IJwtTokenService _jwt;
        private readonly ILogger<MobileAppUserController> _logger;

        public MobileAppUserController(
            IMobileAppUserService service,
            IJwtTokenService jwt,
            ILogger<MobileAppUserController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/mobileappuser/1
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "id": 1,
        ///     "employeeId": 1001,
        ///     "role": "Admin",
        ///     "isActive": true,
        ///     "createdAt": "2024-01-15T10:30:00Z",
        ///     "updatedAt": null
        /// }
        /// </remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MobileAppUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Запрос на получение пользователя по ID: {id}", id);
                var user = await _service.GetByIdAsync(id);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя по ID: {id}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/mobileappuser
        /// 
        /// Ответ 200 OK:
        /// [
        ///     {
        ///         "id": 1,
        ///         "employeeId": 1001,
        ///         "role": "Admin",
        ///         "isActive": true
        ///     }
        /// ]
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MobileAppUserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Запрос на получение всех пользователей");
                var users = await _service.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех пользователей");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Создать нового пользователя
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// POST /api/v1/mobileappuser
        /// {
        ///     "employeeId": 1002,
        ///     "password": "securePassword123",
        ///     "role": "Worker"
        /// }
        /// 
        /// Ответ 201 Created:
        /// {
        ///     "id": 2,
        ///     "employeeId": 1002,
        ///     "role": "Worker",
        ///     "isActive": true,
        ///     "createdAt": "2024-01-15T11:00:00Z"
        /// }
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(MobileAppUserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateMobileAppUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Запрос на создание пользователя для сотрудника {EmployeeId}", dto.EmployeeId);
                var user = await _service.AddAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя для сотрудника {EmployeeId}", dto.EmployeeId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Удалить пользователя по ID
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// DELETE /api/v1/mobileappuser/1
        /// 
        /// Ответ 204 No Content
        /// </remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Запрос на удаление пользователя по ID: {id}", id);
                var result = await _service.DeleteAsync(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя по ID: {id}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить пользователя по ID сотрудника
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/mobileappuser/employee/1001
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "id": 1,
        ///     "employeeId": 1001,
        ///     "role": "Admin",
        ///     "isActive": true
        /// }
        /// </remarks>
        [HttpGet("employee/{employeeId}")]
        [ProducesResponseType(typeof(MobileAppUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByEmployeeId(int employeeId)
        {
            try
            {
                _logger.LogInformation("Запрос на получение пользователя по ID сотрудника: {employeeId}", employeeId);
                var user = await _service.GetByEmployeeIdAsync(employeeId);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя по ID сотрудника: {employeeId}", employeeId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Обновить пароль пользователя
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// PATCH /api/v1/mobileappuser/1/password
        /// {
        ///     "password": "newSecurePassword456"
        /// }
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "id": 1,
        ///     "employeeId": 1001,
        ///     "role": "Admin",
        ///     "isActive": true,
        ///     "updatedAt": "2024-01-15T11:30:00Z"
        /// }
        /// </remarks>
        [HttpPatch("{id}/password")]
        [ProducesResponseType(typeof(MobileAppUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdateMobileUserPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Запрос на обновление пароля для пользователя ID: {id}", id);
                var user = await _service.UpdatePasswordAsync(id, dto);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении пароля для пользователя ID: {id}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Обновить роль пользователя
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// PATCH /api/v1/mobileappuser/1/role
        /// {
        ///     "role": "Supervisor"
        /// }
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "id": 1,
        ///     "employeeId": 1001,
        ///     "role": "Supervisor",
        ///     "isActive": true,
        ///     "updatedAt": "2024-01-15T11:45:00Z"
        /// }
        /// </remarks>
        [HttpPatch("{id}/role")]
        [ProducesResponseType(typeof(MobileAppUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateMobileUserRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Запрос на обновление роли для пользователя ID: {id}", id);
                var user = await _service.UpdateRoleAsync(id, dto);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении роли для пользователя ID: {id}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Обновить активность пользователя
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// PATCH /api/v1/mobileappuser/1/active
        /// {
        ///     "isActive": false
        /// }
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "id": 1,
        ///     "employeeId": 1001,
        ///     "role": "Admin",
        ///     "isActive": false,
        ///     "updatedAt": "2024-01-15T12:00:00Z"
        /// }
        /// </remarks>
        [HttpPatch("{id}/active")]
        [ProducesResponseType(typeof(MobileAppUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateActive(int id, [FromBody] UpdateMobileUserActiveDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Запрос на обновление активности для пользователя ID: {id}", id);
                var user = await _service.UpdateActiveAsync(id, dto);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении активности для пользователя ID: {id}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        ///// <summary>
        ///// Проверить учетные данные пользователя
        ///// </summary>
        ///// <remarks>
        ///// Пример запроса:
        ///// POST /api/v1/mobileappuser/validate
        ///// {
        /////     "username": "1001",
        /////     "password": "securePassword123"
        ///// }
        ///// 
        ///// Ответ 200 OK:
        ///// {
        /////     "id": 1,
        /////     "employeeId": 1001,
        /////     "role": "Admin",
        /////     "isActive": true
        ///// }
        ///// </remarks>
        //[HttpPost("validate")]
        //[ProducesResponseType(typeof(MobileAppUserDto), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //public async Task<IActionResult> ValidateCredentials([FromBody] ValidateCredentialsDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    try
        //    {
        //        _logger.LogInformation("Запрос на проверку учетных данных для пользователя: {username}", dto.Username);
        //        var user = await _service.ValidateCredentialsAsync(dto.Username, dto.Password);
        //        return Ok(user);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return Unauthorized();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Ошибка при проверке учетных данных для пользователя: {username}", dto.Username);
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}

        /// <summary>
        /// Логин: проверка employeeId/password и выдача JWT.
        /// Пример:
        /// POST /api/v1/mobileappuser/login
        /// { "username": "1001", "password": "secret" }
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] ValidateCredentialsDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Запрос входа для пользователя: {username}", dto.Username);

                // 1) Валидируем учётные данные существующим сервисом
                var user = await _service.ValidateCredentialsAsync(dto.Username, dto.Password);

                // 2) Генерируем JWT
                var token = _jwt.CreateToken(user.EmployeeId, user.Role);

                // 3) Отдаём токен + user (удобно для мобильного клиента)
                return Ok(new LoginResponseDto
                {
                    AccessToken = token,
                    User = user
                });
            }
            catch (InvalidOperationException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка входа для пользователя: {username}", dto.Username);
                return BadRequest(new { error = ex.Message });
            }
        }


        /// <summary>
        /// Получить статистику пользователей
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/mobileappuser/statistics
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "totalUsers": 50,
        ///     "activeUsers": 45,
        ///     "inactiveUsers": 5,
        ///     "roles": {
        ///         "Admin": 3,
        ///         "Supervisor": 7,
        ///         "Worker": 40
        ///     }
        /// }
        /// </remarks>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                _logger.LogInformation("Запрос на получение статистики пользователей");
                var statistics = await _service.GetStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики пользователей");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить пользователей по роли
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/mobileappuser/role/Worker
        /// 
        /// Ответ 200 OK:
        /// [
        ///     {
        ///         "id": 2,
        ///         "employeeId": 1002,
        ///         "role": "Worker",
        ///         "isActive": true
        ///     }
        /// ]
        /// </remarks>
        [HttpGet("role/{role}")]
        [ProducesResponseType(typeof(IEnumerable<MobileAppUserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByRole(string role)
        {
            try
            {
                _logger.LogInformation("Запрос на получение пользователей с ролью: {role}", role);
                var users = await _service.GetByRoleAsync(role);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователей с ролью: {role}", role);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Сбросить пароль пользователя
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// POST /api/v1/mobileappuser/reset-password
        /// {
        ///     "employeeId": 1001,
        ///     "newPassword": "temporaryPassword789"
        /// }
        /// 
        /// Ответ 200 OK:
        /// {
        ///     "success": true,
        ///     "message": "Пароль успешно сброшен"
        /// }
        /// </remarks>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Запрос на сброс пароля для пользователя с ID сотрудника: {employeeId}",
                                     dto.EmployeeId);
                var result = await _service.ResetPasswordAsync(dto.EmployeeId, dto.NewPassword);
                return Ok(new { success = result, message = "Пароль успешно сброшен" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сбросе пароля для пользователя с ID сотрудника: {employeeId}",
                              dto.EmployeeId);
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO для проверки учетных данных
    /// </summary>
    public record ValidateCredentialsDto
    {
        public string Username { get; init; } = null!;
        public string Password { get; init; } = null!;
    }

    /// <summary>
    /// DTO для сброса пароля
    /// </summary>
    public record ResetPasswordDto
    {
        public int EmployeeId { get; init; }
        public string NewPassword { get; init; } = null!;
    }

    public record LoginResponseDto
    {
        public string AccessToken { get; init; } = null!;
        public string TokenType { get; init; } = "Bearer";
        public MobileAppUserDto User { get; init; } = null!;
    }
}