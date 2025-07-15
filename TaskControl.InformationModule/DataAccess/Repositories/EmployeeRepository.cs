using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Mapper;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DAL.Repositories
{
    public class EmployeeRepository : IRepository<Employee>, IEmployeeRepository
    {
        private readonly IInformationDataConnection _db;
        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(IInformationDataConnection db, ILogger<EmployeeRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск сотрудника по ID: {id}", id);
            try
            {
                var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeesId == id);
                return employee?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сотрудника по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех сотрудников");
            try
            {
                var employeesModel = await _db.Employees.ToListAsync();
                return employeesModel.Select(e => e.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка сотрудников");
                throw;
            }
        }

        public async Task<int> AddAsync(Employee entity)
        {
            _logger.LogInformation("Добавление нового сотрудника: {surname} {name}", entity.Surname, entity.Name);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Установка даты создания
                entity.CreatedAt = DateTime.UtcNow;

                var model = entity.ToModel();
                return await _db.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении сотрудника: {surname} {name}", entity?.Surname, entity?.Name);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Employee entity)
        {
            _logger.LogInformation("Обновление данных сотрудника ID: {id}", entity.EmployeesId);
            try
            {
                if (entity == null)
                    return 0;

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении сотрудника ID: {id}", entity?.EmployeesId);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление сотрудника ID: {id}", id);
            try
            {
                var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeesId == id);
                if (employee is null)
                    return 0;

                return await _db.DeleteAsync(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении сотрудника ID: {id}", id);
                throw;
            }
        }
    }
}