using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.Services;

namespace TaskControl.TaskModule.Application.Interface
{
    /// <summary>
    /// Сервис для управления пользователями мобильного приложения
    /// </summary>
    public interface IMobileAppUserService
    {
        Task<MobileAppUserDto?> GetByIdAsync(int id);
        Task<IEnumerable<MobileAppUserDto>> GetAllAsync();
        Task<MobileAppUserDto> AddAsync(CreateMobileAppUserDto dto);
        Task<MobileAppUserDto> UpdateAsync(int id, UpdateMobileUserDto dto);
        Task<bool> DeleteAsync(int id);
        Task<MobileAppUserDto?> GetByEmployeeIdAsync(int employeeId);
        Task<bool> DeleteByEmployeeIdAsync(int employeeId);
        Task<MobileAppUserDto> UpdatePasswordAsync(int id, UpdateMobileUserPasswordDto dto);
        Task<MobileAppUserDto> UpdateRoleAsync(int id, UpdateMobileUserRoleDto dto);
        Task<MobileAppUserDto> UpdateActiveAsync(int id, UpdateMobileUserActiveDto dto);
        Task<IEnumerable<MobileAppUserDto>> GetByRoleAsync(string role);
        Task<MobileAppUserDto> ValidateCredentialsAsync(string username, string password);
        Task<bool> ResetPasswordAsync(int employeeId, string newPassword);
        Task<Dictionary<string, object>> GetStatisticsAsync();
    }
}
