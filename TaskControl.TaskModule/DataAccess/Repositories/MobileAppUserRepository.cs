using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DAL.Repositories
{
    public class MobileAppUserRepository : IRepository<MobileAppUser>, IMobileAppUserRepository
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<MobileAppUserRepository> _logger;

        public MobileAppUserRepository(ITaskDataConnection db, ILogger<MobileAppUserRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<MobileAppUser?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск пользователя мобильного приложения по ID: {id}", id);
            try
            {
                var user = await _db.MobileAppUsers.FirstOrDefaultAsync(u => u.Id == id);
                return user?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя мобильного приложения по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<MobileAppUser>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех пользователей мобильного приложения");
            try
            {
                var users = await _db.MobileAppUsers.ToListAsync();
                return users.Select(u => u.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка пользователей мобильного приложения");
                throw;
            }
        }

        public async Task<int> AddAsync(MobileAppUser entity)
        {
            _logger.LogInformation("Добавление нового пользователя мобильного приложения: {userId}",
                                 entity?.EmployeeId);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Устанавливаем дату создания, если не задана
                if (entity.CreatedAt == default)
                    entity.CreatedAt = DateTime.UtcNow;

                var model = entity.ToModel();
                return await _db.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении пользователя мобильного приложения: {userId}",
                              entity?.EmployeeId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(MobileAppUser entity)
        {
            _logger.LogInformation("Обновление пользователя мобильного приложения ID: {id}", entity.Id);
            try
            {
                if (entity == null)
                    return 0;

                // Устанавливаем дату обновления
                entity.UpdatedAt = DateTime.UtcNow;

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении пользователя мобильного приложения ID: {id}", entity?.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление пользователя мобильного приложения ID: {id}", id);
            try
            {
                var user = await _db.MobileAppUsers.FirstOrDefaultAsync(u => u.Id == id);
                if (user is null)
                    return 0;

                return await _db.DeleteAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя мобильного приложения ID: {id}", id);
                throw;
            }
        }

        public async Task<MobileAppUser?> GetByEmployeeIdAsync(int employeeId)
        {
            _logger.LogInformation("Поиск пользователя мобильного приложения по ID сотрудника: {employeeId}", employeeId);
            try
            {
                var user = await _db.MobileAppUsers.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
                return user?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске пользователя мобильного приложения по ID сотрудника: {employeeId}",
                              employeeId);
                throw;
            }
        }

        public async Task<int> DeleteByEmployeeIdAsync(int employeeId)
        {
            _logger.LogInformation("Удаление пользователя мобильного приложения по ID сотрудника: {employeeId}",
                                 employeeId);
            try
            {
                var user = await _db.MobileAppUsers.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
                if (user is null)
                    return 0;

                return await _db.DeleteAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя мобильного приложения по ID сотрудника: {employeeId}",
                              employeeId);
                throw;
            }
        }
    }
}