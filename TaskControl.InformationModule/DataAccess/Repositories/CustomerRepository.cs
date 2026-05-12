using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Mapper;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DAL.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IInformationDataConnection _db;
        private readonly ILogger<CustomerRepository> _logger;

        public CustomerRepository(IInformationDataConnection db, ILogger<CustomerRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск клиента по ID: {Id}", id);
            try
            {
                var model = await _db.GetTable<CustomerModel>()
                    .FirstOrDefaultAsync(c => c.CustomerId == id);
                return model?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске клиента по ID: {Id}", id);
                throw;
            }
        }

        public async Task<Customer?> GetByPhoneAsync(string phone)
        {
            _logger.LogInformation("Поиск клиента по номеру телефона: {Phone}", phone);
            try
            {
                var model = await _db.GetTable<CustomerModel>()
                    .FirstOrDefaultAsync(c => c.Phone == phone);
                return model?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске клиента по номеру телефона: {Phone}", phone);
                throw;
            }
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Поиск клиента по Email: {Email}", email);
            try
            {
                var model = await _db.GetTable<CustomerModel>()
                    .FirstOrDefaultAsync(c => c.Email == email);
                return model?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске клиента по Email: {Email}", email);
                throw;
            }
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            _logger.LogInformation("Запрос списка всех клиентов");
            try
            {
                var models = await _db.GetTable<CustomerModel>().ToListAsync();
                return models.Select(m => m.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка всех клиентов");
                throw;
            }
        }

        public async Task<int> AddAsync(Customer entity)
        {
            _logger.LogInformation("Добавление нового клиента с телефоном {Phone}", entity.Phone);
            try
            {
                var model = entity.ToModel();
                return await _db.InsertWithInt32IdentityAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении нового клиента {Phone}", entity.Phone);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Customer entity)
        {
            _logger.LogInformation("Обновление данных клиента ID: {Id}", entity.CustomerId);
            try
            {
                return await _db.UpdateAsync(entity.ToModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении данных клиента ID: {Id}", entity.CustomerId);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление клиента ID: {Id}", id);
            try
            {
                return await _db.GetTable<CustomerModel>()
                    .Where(c => c.CustomerId == id)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении клиента ID: {Id}", id);
                throw;
            }
        }
    }
}