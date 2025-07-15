using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Mapper;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DAL.Repositories
{
    public class CheckIOEmployeeRepository : IRepository<CheckIOEmployee>
    {
        private readonly IInformationDataConnection _db;
        private readonly ILogger<CheckIOEmployeeRepository> _logger;

        public CheckIOEmployeeRepository(IInformationDataConnection db, ILogger<CheckIOEmployeeRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<CheckIOEmployee?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск записи учета прихода на работу сотрудника по ID: {id}", id);
            try
            {
                var checkIOEmployeesModels = await _db.CheckIOEmployees.FirstOrDefaultAsync(e => e.Id == id);
                return checkIOEmployeesModels?.ToDomain();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении записи учета прихода на работу сотрудника по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<CheckIOEmployee>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех записей учета прихода на работу сотрудника");
            try
            {
                var checkIOEmployeesModels = await _db.CheckIOEmployees.ToListAsync();
                return checkIOEmployeesModels.Select(e => e.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех записей учета прихода на работу сотрудника");
                throw;
            }
        }

        public async Task<int> AddAsync(CheckIOEmployee entity)
        {
            _logger.LogInformation("Добавление записи учета прихода на работу сотрудника");
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var model = entity.ToModel();
                return await _db.InsertAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении записи учета прихода на работу сотрудника");
                throw;
            }
        }

        public async Task<int> UpdateAsync(CheckIOEmployee entity)
        {
            _logger.LogInformation("Обновление записи учета прихода на работу сотрудника ID: {id}", entity.Id);
            try
            {
                if (entity == null)
                    return 0;

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении записи учета прихода на работу сотрудника ID: {id}", entity?.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление записи CheckIOEmployee ID: {id}", id);
            try
            {
                var record = await _db.CheckIOEmployees.FirstOrDefaultAsync(e => e.Id == id);
                if (record is null)
                    return 0;

                return await _db.DeleteAsync(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении записи CheckIOEmployee ID: {id}", id);
                throw;
            }
        }
    }
}