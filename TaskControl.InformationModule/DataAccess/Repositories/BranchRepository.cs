using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Mapper;
using TaskControl.InformationModule.Domain;


namespace TaskControl.InformationModule.DataAccess.Repositories
{
    public class BranchRepository : IRepository<Branch>, IBranchRepository
    {
        private readonly IInformationDataConnection _db;
        private readonly ILogger<BranchRepository> _logger;

        public BranchRepository(IInformationDataConnection db, ILogger<BranchRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<Branch?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск филиала по ID: {id}", id);
            try
            {
                var branch = await _db.Branches.FirstOrDefaultAsync(b => b.BranchId == id);
                return branch?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении филиала по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Branch>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех филиалов");
            try
            {
                var branchesModel = await _db.Branches.ToListAsync();
                return branchesModel.Select(b => b.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка филиалов");
                throw;
            }
        }

        public async Task<int> AddAsync(Branch entity)
        {
            _logger.LogInformation("Добавление филиала: {name}", entity.BranchName);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Установка временных меток
                entity.CreatedAt = DateTime.UtcNow;

                return await _db.InsertAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении филиала: {name}", entity?.BranchName);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Branch entity)
        {
            _logger.LogInformation("Обновление филиала ID: {id}", entity.BranchId);
            try
            {
                if (entity == null)
                    return 0;

                return await _db.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении филиала ID: {id}", entity?.BranchId);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление филиала ID: {id}", id);
            try
            {
                var branch = await _db.Branches.FirstOrDefaultAsync(b => b.BranchId == id);
                if (branch is null)
                    return 0;

                return await _db.DeleteAsync(branch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении филиала ID: {id}", id);
                throw;
            }
        }
    }
}
