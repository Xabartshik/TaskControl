using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.DataAccess.Interface; // Добавлено для ICheckIOEmployeeRepository
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
        private readonly ICustomerService _customerService;
        private readonly ICheckIOEmployeeRepository _checkIORepository; // Новая зависимость
        private readonly ILogger<MobileAppUserService> _logger;

        public MobileAppUserService(
            IMobileAppUserRepository repository,
            ILogger<MobileAppUserService> logger,
            ICustomerService customerService,
            ICheckIOEmployeeRepository checkIORepository) // Добавлено в конструктор
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _checkIORepository = checkIORepository ?? throw new ArgumentNullException(nameof(checkIORepository));
        }

        /// <summary>
        /// Получает ID последнего филиала, в котором сотрудник отметился "in"
        /// </summary>
        private async Task<int?> GetLastBranchIdByMarkInAsync(int employeeId)
        {
            _logger.LogDebug("Поиск последнего филиала отметки 'in' для сотрудника: {EmployeeId}", employeeId);
            try
            {
                // Получаем все отметки сотрудника
                var checkIns = await _checkIORepository.GetAllAsync(); // Предполагается наличие метода GetAllAsync или GetByEmployeeIdAsync

                var lastMarkIn = checkIns
                    .Where(c => c.EmployeeId == employeeId && c.CheckType.Equals("in", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(c => c.CheckTimeStamp)
                    .FirstOrDefault();

                return lastMarkIn?.BranchId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении последнего филиала для сотрудника {EmployeeId}", employeeId);
                return null;
            }
        }

        public async Task<MobileAppUserDto> RegisterCustomerAsync(RegisterCustomerRequestDto request)
        {
            _logger.LogInformation("Начало регистрации нового покупателя с логином: {Login}", request.Login);

            try
            {
                // 1. Проверка обязательных полей
                if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new ArgumentException("Логин и пароль обязательны для заполнения.");
                }

                if (string.IsNullOrWhiteSpace(request.Phone) && string.IsNullOrWhiteSpace(request.Email))
                {
                    throw new ArgumentException("Необходимо указать хотя бы номер телефона или Email.");
                }

                // 2. Проверка уникальности логина в MobileAppUser
                var existingUser = await _repository.GetByLoginAsync(request.Login);
                if (existingUser != null)
                {
                    _logger.LogWarning("Попытка регистрации с уже существующим логином: {Login}", request.Login);
                    throw new InvalidOperationException("Пользователь с таким логином уже существует.");
                }

                // 3. Создание профиля Customer через CustomerService
                var customerDto = await _customerService.CreateCustomerAsync(new TaskControl.InformationModule.Application.DTOs.CustomerDto
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    Email = request.Email
                });

                if (customerDto == null || customerDto.CustomerId == 0)
                {
                    throw new Exception("Не удалось создать профиль покупателя.");
                }

                // 4. Создание учетной записи MobileAppUser
                var passwordHash = HashPassword(request.Password);

                var newUser = new MobileAppUser(
                    login: request.Login,
                    passwordHash: passwordHash,
                    role: MobileUserRole.Customer,
                    employeeId: null,
                    customerId: customerDto.CustomerId,
                    branchId: null
                );

                var newUserId = await _repository.AddAsync(newUser);
                newUser.Id = newUserId;

                _logger.LogInformation("Успешно зарегистрирован покупатель. UserId: {UserId}, CustomerId: {CustomerId}",
                    newUserId, customerDto.CustomerId);

                return MobileAppUserDto.ToDto(newUser);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка в RegisterCustomerAsync для {Login}", request.Login);
                throw new Exception("Не удалось завершить регистрацию из-за системной ошибки.");
            }
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
            _logger.LogInformation("Регистрация пользователя с логином: {Login}", dto.Login);

            var existing = await _repository.GetByLoginAsync(dto.Login);
            if (existing != null)
                throw new InvalidOperationException($"Логин {dto.Login} уже занят");

            var passwordHash = HashPassword(dto.Password);

            // Если это сотрудник, пытаемся найти его последний филиал по отметке "in"
            int? effectiveBranchId = dto.BranchId;
            if (dto.EmployeeId.HasValue && effectiveBranchId == null)
            {
                effectiveBranchId = await GetLastBranchIdByMarkInAsync(dto.EmployeeId.Value);
            }

            var user = new MobileAppUser(
                login: dto.Login,
                passwordHash: passwordHash,
                role: dto.Role,
                employeeId: dto.EmployeeId,
                customerId: dto.CustomerId,
                branchId: effectiveBranchId
            );

            var userId = await _repository.AddAsync(user);
            user.Id = userId;

            return MobileAppUserDto.ToDto(user);
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

                var role = dto.Role;

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
                _logger.LogError(ex, "Ошибка при получении данных для {id}", id);
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

        public async Task<MobileAppUserDto?> ValidateCredentialsAsync(string identifier, string password)
        {
            _logger.LogInformation("Попытка авторизации для идентификатора: {Identifier}", identifier);

            try
            {
                if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
                    return null;

                MobileAppUser? user = null;

                bool isEmail = Regex.IsMatch(identifier, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                bool isPhone = Regex.IsMatch(identifier, @"^\+?[\d\s\-\(\)]{10,20}$");

                if (isEmail || isPhone)
                {
                    _logger.LogDebug("Идентификатор '{Identifier}' распознан как {Type}",
                        identifier, isEmail ? "Email" : "Телефон");

                    var customer = isEmail
                        ? await _customerService.GetByEmailAsync(identifier)
                        : await _customerService.GetByPhoneAsync(identifier.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", ""));

                    if (customer != null && customer.CustomerId > 0)
                    {
                        var allUsers = await _repository.GetAllAsync();
                        user = allUsers.FirstOrDefault(u => u.CustomerId == customer.CustomerId);
                    }
                }
                else
                {
                    _logger.LogDebug("Идентификатор '{Identifier}' распознан как обычный Логин", identifier);
                    user = await _repository.GetByLoginAsync(identifier);
                }

                if (user == null)
                {
                    _logger.LogWarning("Пользователь с идентификатором {Identifier} не найден", identifier);
                    return null;
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Попытка входа деактивированного пользователя: {Login}", user.Login);
                    return null;
                }

                string inputHash = HashPassword(password);
                if (user.PasswordHash != inputHash)
                {
                    _logger.LogWarning("Неверный пароль для пользователя: {Login}", user.Login);
                    return null;
                }

                // При успешной валидации сотрудника, обновляем его BranchId по последней отметке "in", если он не задан
                if (user.EmployeeId.HasValue && user.BranchId == null)
                {
                    var lastBranchId = await GetLastBranchIdByMarkInAsync(user.EmployeeId.Value);
                    if (lastBranchId.HasValue)
                    {
                        user.BranchId = lastBranchId;
                        await _repository.UpdateAsync(user);
                        _logger.LogInformation("Обновлен BranchId ({BranchId}) для пользователя {Login} на основе последней отметки 'in'",
                            lastBranchId, user.Login);
                    }
                }

                _logger.LogInformation("Успешная авторизация пользователя: {Login}", user.Login);
                return MobileAppUserDto.ToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при валидации учетных данных для {Identifier}", identifier);
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

    public record UpdateMobileUserDto
    {
        public string? Role { get; init; }
    }
}