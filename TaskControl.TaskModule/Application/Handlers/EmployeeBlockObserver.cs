using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.DTOs;

namespace TaskControl.TaskModule.Application.Handlers
{
    /// <summary>
    /// Наблюдатель блокировки сотрудников для управления их аккаунтами мобильного приложения
    /// </summary>
    public class EmployeeBlockObserver : IEmployeeBlockObserver
    {
        private readonly IMobileAppUserService _userService;
        private readonly ILogger<EmployeeBlockObserver> _logger;

        public EmployeeBlockObserver(IMobileAppUserService userService, ILogger<EmployeeBlockObserver> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Автоматическая деактивация аккаунта пользователя при блокировке сотрудника
        /// </summary>
        public async Task OnEmployeeBlockedAsync(int employeeId)
        {
            _logger.LogInformation("Обработка события блокировки сотрудника {EmployeeId}", employeeId);
            try
            {
                var user = await _userService.GetByEmployeeIdAsync(employeeId);
                if (user != null)
                {
                    _logger.LogInformation("Деактивация аккаунта пользователя {Login} (ID: {UserId}) для сотрудника {EmployeeId}", user.Login, user.Id, employeeId);
                    await _userService.UpdateActiveAsync(user.Id, new UpdateMobileUserActiveDto { IsActive = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при автоматической деактивации аккаунта сотрудника {EmployeeId}", employeeId);
            }
        }

        /// <summary>
        /// Автоматическая активация аккаунта пользователя при разблокировке сотрудника
        /// </summary>
        public async Task OnEmployeeUnblockedAsync(int employeeId)
        {
            _logger.LogInformation("Обработка события разблокировки сотрудника {EmployeeId}", employeeId);
            try
            {
                var user = await _userService.GetByEmployeeIdAsync(employeeId);
                if (user != null)
                {
                    _logger.LogInformation("Активация аккаунта пользователя {Login} (ID: {UserId}) для сотрудника {EmployeeId}", user.Login, user.Id, employeeId);
                    await _userService.UpdateActiveAsync(user.Id, new UpdateMobileUserActiveDto { IsActive = true });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при автоматической активации аккаунта сотрудника {EmployeeId}", employeeId);
            }
        }
    }
}
