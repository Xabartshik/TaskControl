using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services
{
    /// <summary>
    /// Реализация сервиса управления пользователями мобильного приложения
    /// </summary>
    public class MobileAppUserService : IMobileAppUserService
    {
        private readonly IMobileAppUserRepository _repository;
        private readonly ILogger<MobileAppUserService> _logger;

        public MobileAppUserService(
            IMobileAppUserRepository repository,
            ILogger<MobileAppUserService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MobileAppUserDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Получение пользователя мобильного приложения по ID: {id}", id);
            try
            {
                var user = await _repository.GetByIdAsync(id);
                return user != null ? MobileAppUserDto.ToDto(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя мобильного приложения по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<MobileAppUserDto>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех пользователей мобильного приложения");
            try
            {
                var users = await _repository.GetAllAsync();
                return users.Select(u => MobileAppUserDto.ToDto(u));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка пользователей мобильного приложения");
                throw;
            }
        }

        public async Task<MobileAppUserDto> AddAsync(CreateMobileAppUserDto dto)
        {
            _logger.LogInformation("Добавление нового пользователя мобильного приложения для сотрудника {EmployeeId}",
                                 dto.EmployeeId);
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                // Хеширование пароля
                var passwordHash = HashPassword(dto.Password);

                // Создание доменного объекта
                var user = new MobileAppUser
                {
                    EmployeeId = dto.EmployeeId,
                    PasswordHash = passwordHash,
                    Role = Enum.TryParse<MobileUserRole>(dto.Role, ignoreCase: true, out var parsedRole)
                           ? parsedRole
                           : MobileUserRole.Worker,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Проверка существования пользователя с таким EmployeeId
                var existingUser = await _repository.GetByEmployeeIdAsync(dto.EmployeeId);
                if (existingUser != null)
                    throw new InvalidOperationException($"Пользователь с EmployeeId {dto.EmployeeId} уже существует");

                var userId = await _repository.AddAsync(user);
                user.Id = userId;

                _logger.LogInformation("Пользователь мобильного приложения добавлен с ID: {userId}", userId);

                return MobileAppUserDto.ToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении пользователя мобильного приложения для сотрудника {EmployeeId}",
                              dto?.EmployeeId);
                throw;
            }
        }

        public async Task<MobileAppUserDto> UpdateAsync(int id, UpdateMobileUserDto dto)
        {
            _logger.LogInformation("Обновление пользователя мобильного приложения ID: {id}", id);
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var user = await _repository.GetByIdAsync(id);
                if (user == null)
                    throw new InvalidOperationException($"Пользователь с ID {id} не найден");

                if (dto.Role != null)
                {
                    var role = Enum.TryParse<MobileUserRole>(dto.Role, ignoreCase: true, out var parsedRole)
                               ? parsedRole
                               : user.Role;
                    user.Role = role;
                }

                user.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(user);

                _logger.LogInformation("Пользователь мобильного приложения ID: {id} обновлен", id);

                return MobileAppUserDto.ToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении пользователя мобильного приложения ID: {id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление пользователя мобильного приложения ID: {id}", id);
            try
            {
                var result = await _repository.DeleteAsync(id);
                var success = result > 0;

                _logger.LogInformation("Пользователь мобильного приложения ID: {id} удален: {success}", id, success);
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя мобильного приложения ID: {id}", id);
                throw;
            }
        }

        public async Task<MobileAppUserDto?> GetByEmployeeIdAsync(int employeeId)
        {
            _logger.LogInformation("Получение пользователя мобильного приложения по ID сотрудника: {employeeId}",
                                 employeeId);
            try
            {
                var user = await _repository.GetByEmployeeIdAsync(employeeId);
                return user != null ? MobileAppUserDto.ToDto(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя мобильного приложения по ID сотрудника: {employeeId}",
                              employeeId);
                throw;
            }
        }

        public async Task<bool> DeleteByEmployeeIdAsync(int employeeId)
        {
            _logger.LogInformation("Удаление пользователя мобильного приложения по ID сотрудника: {employeeId}",
                                 employeeId);
            try
            {
                var result = await _repository.DeleteByEmployeeIdAsync(employeeId);
                var success = result > 0;

                _logger.LogInformation("Пользователь мобильного приложения по ID сотрудника: {employeeId} удален: {success}",
                                     employeeId, success);
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя мобильного приложения по ID сотрудника: {employeeId}",
                              employeeId);
                throw;
            }
        }

        public async Task<MobileAppUserDto> UpdatePasswordAsync(int id, UpdateMobileUserPasswordDto dto)
        {
            _logger.LogInformation("Обновление пароля для пользователя мобильного приложения ID: {id}", id);
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var user = await _repository.GetByIdAsync(id);
                if (user == null)
                    throw new InvalidOperationException($"Пользователь с ID {id} не найден");

                // Хеширование нового пароля
                var newPasswordHash = HashPassword(dto.Password);
                user.PasswordHash = newPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(user);

                _logger.LogInformation("Пароль для пользователя мобильного приложения ID: {id} обновлен", id);

                return MobileAppUserDto.ToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении пароля для пользователя мобильного приложения ID: {id}", id);
                throw;
            }
        }

        public async Task<MobileAppUserDto> UpdateRoleAsync(int id, UpdateMobileUserRoleDto dto)
        {
            _logger.LogInformation("Обновление роли для пользователя мобильного приложения ID: {id}", id);
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var user = await _repository.GetByIdAsync(id);
                if (user == null)
                    throw new InvalidOperationException($"Пользователь с ID {id} не найден");

                var role = Enum.TryParse<MobileUserRole>(dto.Role, ignoreCase: true, out var parsedRole)
                           ? parsedRole
                           : throw new ArgumentException($"Недопустимая роль: {dto.Role}");

                user.Role = role;
                user.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(user);

                _logger.LogInformation("Роль для пользователя мобильного приложения ID: {id} обновлена на {role}",
                                     id, role);

                return MobileAppUserDto.ToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении роли для пользователя мобильного приложения ID: {id}", id);
                throw;
            }
        }

        public async Task<MobileAppUserDto> UpdateActiveAsync(int id, UpdateMobileUserActiveDto dto)
        {
            _logger.LogInformation("Обновление активности для пользователя мобильного приложения ID: {id}", id);
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var user = await _repository.GetByIdAsync(id);
                if (user == null)
                    throw new InvalidOperationException($"Пользователь с ID {id} не найден");

                user.IsActive = dto.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(user);

                _logger.LogInformation("Активность для пользователя мобильного приложения ID: {id} обновлена на {isActive}",
                                     id, dto.IsActive);

                return MobileAppUserDto.ToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении активности для пользователя мобильного приложения ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<MobileAppUserDto>> GetByRoleAsync(string role)
        {
            _logger.LogInformation("Получение пользователей мобильного приложения с ролью: {role}", role);
            try
            {
                var allUsers = await _repository.GetAllAsync();
                var filteredUsers = allUsers.Where(u => u.Role.ToString().Equals(role, StringComparison.OrdinalIgnoreCase));

                return filteredUsers.Select(u => MobileAppUserDto.ToDto(u));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователей мобильного приложения с ролью: {role}", role);
                throw;
            }
        }

        public async Task<MobileAppUserDto> ValidateCredentialsAsync(string username, string password)
        {
            _logger.LogInformation("Проверка учетных данных для пользователя: {username}", username);
            try
            {
                // В данном контексте username - это employeeId
                if (!int.TryParse(username, out var employeeId))
                    throw new ArgumentException("Некорректный формат имени пользователя");

                var user = await _repository.GetByEmployeeIdAsync(employeeId);
                if (user == null)
                    throw new InvalidOperationException("Пользователь не найден");

                if (!user.IsActive)
                    throw new InvalidOperationException("Пользователь деактивирован");

                var passwordHash = HashPassword(password);
                if (user.PasswordHash != passwordHash)
                    throw new InvalidOperationException("Неверный пароль");

                _logger.LogInformation("Учетные данные для пользователя: {username} успешно проверены", username);

                return MobileAppUserDto.ToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке учетных данных для пользователя: {username}", username);
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(int employeeId, string newPassword)
        {
            _logger.LogInformation("Сброс пароля для пользователя с ID сотрудника: {employeeId}", employeeId);
            try
            {
                var user = await _repository.GetByEmployeeIdAsync(employeeId);
                if (user == null)
                    throw new InvalidOperationException($"Пользователь с EmployeeId {employeeId} не найден");

                var newPasswordHash = HashPassword(newPassword);
                user.PasswordHash = newPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(user);

                _logger.LogInformation("Пароль для пользователя с ID сотрудника: {employeeId} успешно сброшен",
                                     employeeId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сбросе пароля для пользователя с ID сотрудника: {employeeId}",
                              employeeId);
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetStatisticsAsync()
        {
            _logger.LogInformation("Получение статистики пользователей мобильного приложения");
            try
            {
                var allUsers = await _repository.GetAllAsync();

                var statistics = new Dictionary<string, object>
                {
                    { "totalUsers", allUsers.Count() },
                    { "activeUsers", allUsers.Count(u => u.IsActive) },
                    { "inactiveUsers", allUsers.Count(u => !u.IsActive) },
                    { "roles", allUsers.GroupBy(u => u.Role)
                                      .ToDictionary(g => g.Key.ToString(), g => g.Count()) },
                    { "avgAccountAgeDays", allUsers.Any()
                        ? allUsers.Average(u => (DateTime.UtcNow - u.CreatedAt).TotalDays)
                        : 0 }
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики пользователей мобильного приложения");
                throw;
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    /// <summary>
    /// DTO для обновления пользователя
    /// </summary>
    public record UpdateMobileUserDto
    {
        public string? Role { get; init; }
    }
}