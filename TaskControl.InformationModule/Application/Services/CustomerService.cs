using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            ICustomerRepository repository,
            ILogger<CustomerService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Запрос данных клиента по ID: {Id}", id);
            try
            {
                var customer = await _repository.GetByIdAsync(id);
                return customer != null ? CustomerDto.ToDto(customer) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиента по ID: {Id}", id);
                throw;
            }
        }

        public async Task<CustomerDto?> GetByPhoneAsync(string phone)
        {
            _logger.LogInformation("Запрос данных клиента по телефону: {Phone}", phone);
            try
            {
                var customer = await _repository.GetByPhoneAsync(phone);
                return customer != null ? CustomerDto.ToDto(customer) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиента по телефону: {Phone}", phone);
                throw;
            }
        }

        public async Task<CustomerDto?> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Запрос данных клиента по Email: {Email}", email);
            try
            {
                var customer = await _repository.GetByEmailAsync(email);
                return customer != null ? CustomerDto.ToDto(customer) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиента по Email: {Email}", email);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            _logger.LogInformation("Запрос списка всех клиентов");
            try
            {
                var customers = await _repository.GetAllAsync();
                return customers.Select(CustomerDto.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка всех клиентов");
                throw;
            }
        }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerDto dto)
        {
            _logger.LogInformation("Начало создания профиля покупателя: {Phone}", dto.Phone);
            try
            {
                // 1. Бизнес-проверка: уникальность телефона
                var existingPhone = await _repository.GetByPhoneAsync(dto.Phone);
                if (existingPhone != null)
                {
                    _logger.LogWarning("Попытка регистрации дубликата телефона: {Phone}", dto.Phone);
                    throw new InvalidOperationException("Клиент с таким номером телефона уже существует.");
                }

                // 2. Бизнес-проверка: уникальность Email
                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var existingEmail = await _repository.GetByEmailAsync(dto.Email);
                    if (existingEmail != null)
                    {
                        _logger.LogWarning("Попытка регистрации дубликата Email: {Email}", dto.Email);
                        throw new InvalidOperationException("Клиент с таким Email уже существует.");
                    }
                }

                var customer = new Customer
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    CreatedAt = DateTime.UtcNow
                };

                var id = await _repository.AddAsync(customer);
                customer.CustomerId = id;

                _logger.LogInformation("Успешно создан клиент ID: {Id}, Телефон: {Phone}", id, dto.Phone);
                return CustomerDto.ToDto(customer);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Критическая ошибка при создании клиента: {Phone}", dto.Phone);
                throw;
            }
            catch (Exception) { throw; }
        }

        public async Task<CustomerDto> UpdateCustomerAsync(int id, CustomerDto dto)
        {
            _logger.LogInformation("Начало обновления профиля покупателя ID: {Id}", id);
            try
            {
                var customer = await _repository.GetByIdAsync(id);
                if (customer == null)
                {
                    _logger.LogWarning("Клиент для обновления не найден. ID: {Id}", id);
                    throw new InvalidOperationException("Покупатель не найден.");
                }

                // Здесь можно добавить проверку на уникальность телефона/email, 
                // если они изменились и не принадлежат другому пользователю.

                customer.FirstName = dto.FirstName;
                customer.LastName = dto.LastName;
                customer.Phone = dto.Phone;
                customer.Email = dto.Email;

                await _repository.UpdateAsync(customer);

                _logger.LogInformation("Профиль клиента {Id} успешно обновлен", id);
                return CustomerDto.ToDto(customer);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Ошибка при обновлении данных клиента ID: {Id}", id);
                throw;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            _logger.LogInformation("Запрос на удаление профиля покупателя ID: {Id}", id);
            try
            {
                int deletedCount = await _repository.DeleteAsync(id);
                if (deletedCount > 0)
                {
                    _logger.LogInformation("Клиент ID: {Id} успешно удален", id);
                    return true;
                }

                _logger.LogWarning("Попытка удаления несуществующего клиента ID: {Id}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении клиента ID: {Id}", id);
                throw;
            }
        }
    }
}