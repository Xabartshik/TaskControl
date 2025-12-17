using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Repositories;

public class InventoryDiscrepancyRepository : IInventoryDiscrepancyRepository
{
    private readonly ITaskDataConnection _db;
    private readonly ILogger<InventoryDiscrepancyRepository> _logger;

    public InventoryDiscrepancyRepository(
        ITaskDataConnection db,
        ILogger<InventoryDiscrepancyRepository> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger;
    }

    public async Task<InventoryDiscrepancy> GetByIdAsync(int id)
    {
        _logger.LogInformation("Получение расхождения по ID: {DiscrepancyId}", id);
        try
        {
            var discrepancy = await _db.InventoryDiscrepancies
                .FirstOrDefaultAsync(d => d.Id == id);
            return discrepancy?.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении расхождения по ID: {DiscrepancyId}", id);
            throw;
        }
    }

    public async Task<List<InventoryDiscrepancy>> GetByAssignmentLineIdAsync(int inventoryAssignmentLineId)
    {
        _logger.LogInformation("Получение расхождений для строки инвентаризации: {LineId}", inventoryAssignmentLineId);
        try
        {
            var discrepancies = await _db.InventoryDiscrepancies
                .Where(d => d.InventoryAssignmentLineId == inventoryAssignmentLineId)
                .ToListAsync();
            return discrepancies.Select(d => d.ToDomain()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении расхождений для строки: {LineId}", inventoryAssignmentLineId);
            throw;
        }
    }

    public async Task<List<InventoryDiscrepancy>> GetByAssignmentIdAsync(int inventoryAssignmentId)
    {
        _logger.LogInformation("Получение расхождений для назначения инвентаризации: {AssignmentId}", inventoryAssignmentId);
        try
        {
            var discrepancies = await _db.InventoryDiscrepancies
                .Join(
                    _db.InventoryAssignmentLines,
                    d => d.InventoryAssignmentLineId,
                    l => l.Id,
                    (d, l) => new { Discrepancy = d, Line = l })
                .Where(x => x.Line.InventoryAssignmentId == inventoryAssignmentId)
                .Select(x => x.Discrepancy)
                .ToListAsync();
            return discrepancies.Select(d => d.ToDomain()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении расхождений для назначения: {AssignmentId}", inventoryAssignmentId);
            throw;
        }
    }

    public async Task<List<InventoryDiscrepancy>> GetPendingAsync()
    {
        _logger.LogInformation("Получение нерешённых расхождений");
        try
        {
            var discrepancies = await _db.InventoryDiscrepancies
                .Where(d => d.ResolutionStatus == (int)DiscrepancyResolutionStatus.Pending)
                .ToListAsync();
            return discrepancies.Select(d => d.ToDomain()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении нерешённых расхождений");
            throw;
        }
    }

    public async Task<List<InventoryDiscrepancy>> GetByTypeAsync(DiscrepancyType type)
    {
        _logger.LogInformation("Получение расхождений типа: {Type}", type);
        try
        {
            var discrepancies = await _db.InventoryDiscrepancies
                .Where(d => d.Type == (int)type)
                .ToListAsync();
            return discrepancies.Select(d => d.ToDomain()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении расхождений типа: {Type}", type);
            throw;
        }
    }

    public async Task<int> AddAsync(InventoryDiscrepancy discrepancy)
    {
        if (discrepancy is null)
            throw new ArgumentNullException(nameof(discrepancy));

        _logger.LogInformation("Создание нового расхождения для строки: {LineId}", discrepancy.InventoryAssignmentLineId);
        try
        {
            var model = discrepancy.ToModel();
            return await _db.InsertAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании расхождения для строки: {LineId}", discrepancy.InventoryAssignmentLineId);
            throw;
        }
    }

    public async Task<int> UpdateAsync(InventoryDiscrepancy discrepancy)
    {
        if (discrepancy is null)
            throw new ArgumentNullException(nameof(discrepancy));

        _logger.LogInformation("Обновление расхождения ID: {DiscrepancyId}, новый статус: {Status}", 
            discrepancy.Id, discrepancy.ResolutionStatus);
        try
        {
            var model = discrepancy.ToModel();
            return await _db.UpdateAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении расхождения ID: {DiscrepancyId}", discrepancy.Id);
            throw;
        }
    }

    public async Task<int> DeleteAsync(int id)
    {
        _logger.LogInformation("Удаление расхождения ID: {DiscrepancyId}", id);
        try
        {
            var discrepancy = await _db.InventoryDiscrepancies
                .FirstOrDefaultAsync(d => d.Id == id);
            if (discrepancy is null)
                return 0;
            return await _db.DeleteAsync(discrepancy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении расхождения ID: {DiscrepancyId}", id);
            throw;
        }
    }

    public async Task<int> GetCountByAssignmentIdAsync(int inventoryAssignmentId)
    {
        _logger.LogInformation("Подсчёт расхождений для назначения: {AssignmentId}", inventoryAssignmentId);
        try
        {
            return await _db.InventoryDiscrepancies
                .Join(
                    _db.InventoryAssignmentLines,
                    d => d.InventoryAssignmentLineId,
                    l => l.Id,
                    (d, l) => new { Discrepancy = d, Line = l })
                .Where(x => x.Line.InventoryAssignmentId == inventoryAssignmentId)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при подсчёте расхождений для назначения: {AssignmentId}", inventoryAssignmentId);
            throw;
        }
    }
}
