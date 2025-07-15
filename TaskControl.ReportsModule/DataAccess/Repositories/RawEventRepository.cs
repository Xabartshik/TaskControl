using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.SharedInterfaces;
using TaskControl.ReportsModule.DataAccess.Interface;
using TaskControl.ReportsModule.DataAccess.Mapper;
using TaskControl.ReportsModule.Domain;

namespace TaskControl.ReportsModule.DataAccess.Repositories
{
    public class RawEventRepository : IRepository<RawEvent>
    {
        private readonly IReportDataConnection _db;
        private readonly ILogger<RawEventRepository> _logger;

        public RawEventRepository(IReportDataConnection db, ILogger<RawEventRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }


        public async Task<RawEvent?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Поиск события по ID: {id}", id);
            try
            {
                var rawEvent = await _db.RawEvents.FirstOrDefaultAsync(e => e.ReportId == id);
                return rawEvent?.ToDomain();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении события по ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RawEvent>> GetAllAsync()
        {
            _logger.LogInformation("Получение всех событий");
            try
            {
                var events = await _db.RawEvents.ToListAsync();
                return events.Select(e => e.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка событий");
                throw;
            }
        }

        public async Task<int> AddAsync(RawEvent entity)
        {
            _logger.LogInformation("Добавление нового события типа {type}", entity.Type);
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (string.IsNullOrWhiteSpace(entity.Type))
                    throw new ArgumentException("Тип события обязателен");

                if (entity.JSONParams == null)
                    throw new ArgumentException("Параметры события обязательны");

                var model = entity.ToModel();
                return await _db.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении события типа {type}", entity?.Type);
                throw;
            }
        }

        public async Task<int> UpdateAsync(RawEvent entity)
        {
            _logger.LogInformation("Обновление события ID: {id}", entity.ReportId);
            try
            {
                if (entity == null)
                    return 0;

                if (string.IsNullOrWhiteSpace(entity.Type))
                    throw new ArgumentException("Тип события обязателен");

                var model = entity.ToModel();
                return await _db.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении события ID: {id}", entity?.ReportId);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            _logger.LogInformation("Удаление события ID: {id}", id);
            try
            {
                var rawEvent = await _db.RawEvents.FirstOrDefaultAsync(e => e.ReportId == id);
                if (rawEvent is null)
                    return 0;

                return await _db.DeleteAsync(rawEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении события ID: {id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RawEvent>> GetByTypeAsync(string type)
        {
            _logger.LogInformation("Получение событий типа: {type}", type);
            try
            {
                var events = await _db.RawEvents
                    .Where(e => e.Type == type)
                    .ToListAsync();

                return events.Select(e => e.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении событий типа: {type}", type);
                throw;
            }
        }

        public async Task<IEnumerable<RawEvent>> GetByTimeRangeAsync(DateTime from, DateTime to)
        {
            _logger.LogInformation("Получение событий за период с {from} по {to}", from, to);
            try
            {
                var events = await _db.RawEvents
                    .Where(e => e.EventTime >= from && e.EventTime <= to)
                    .OrderBy(e => e.EventTime)
                    .ToListAsync();

                return events.Select(e => e.ToDomain());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении событий за период с {from} по {to}", from, to);
                throw;
            }
        }


    }
}