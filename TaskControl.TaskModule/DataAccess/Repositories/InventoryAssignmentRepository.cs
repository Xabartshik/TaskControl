using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Repositories;

public interface IInventoryAssignmentRepository
{
    Task<InventoryAssignment> GetByIdAsync(int id);
    Task<InventoryAssignment> GetByTaskIdAsync(int taskId);
    Task<List<InventoryAssignment>> GetByUserIdAsync(int userId);
    Task<List<InventoryAssignment>> GetByBranchIdAsync(int branchId);
    Task<List<InventoryAssignment>> GetByStatusAsync(InventoryAssignmentStatus status);
    Task<List<InventoryAssignment>> GetActiveAsync();
    Task<int> AddAsync(InventoryAssignment assignment);
    Task<int> UpdateAsync(InventoryAssignment assignment);
}

public class InventoryAssignmentRepository : IInventoryAssignmentRepository
{
    private readonly ITaskDataConnection _db;
    private readonly ILogger<InventoryAssignmentRepository> _logger;

    public InventoryAssignmentRepository(
        ITaskDataConnection db,
        ILogger<InventoryAssignmentRepository> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger;
    }

    public async Task<InventoryAssignment> GetByIdAsync(int id)
    {
        _logger.LogInformation("Получение назначения инвентаризации по ID: {AssignmentId}", id);
        try
        {
            var assignment = await _db.InventoryAssignments
                .FirstOrDefaultAsync(a => a.Id == id);
            return assignment?.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении назначения по ID: {AssignmentId}", id);
            throw;
        }
    }

    public async Task<InventoryAssignment> GetByTaskIdAsync(int taskId)
    {
        _logger.LogInformation("Получение назначения инвентаризации по TaskID: {TaskId}", taskId);
        try
        {
            var assignment = await _db.InventoryAssignments
                .FirstOrDefaultAsync(a => a.TaskId == taskId);
            return assignment?.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении назначения по TaskID: {TaskId}", taskId);
            throw;
        }
    }

    public async Task<List<InventoryAssignment>> GetByUserIdAsync(int userId)
    {
        _logger.LogInformation("Получение назначений для пользователя: {UserId}", userId);
        try
        {
            var assignments = await _db.InventoryAssignments
                .Where(a => a.AssignedToUserId == userId)
                .ToListAsync();
            return assignments.Select(a => a.ToDomain()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении назначений для пользователя: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<InventoryAssignment>> GetByBranchIdAsync(int branchId)
    {
        _logger.LogInformation("Получение назначений для филиала: {BranchId}", branchId);
        try
        {
            var assignments = await _db.InventoryAssignments
                .Where(a => a.BranchId == branchId)
                .ToListAsync();
            return assignments.Select(a => a.ToDomain()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении назначений для филиала: {BranchId}", branchId);
            throw;
        }
    }

    public async Task<List<InventoryAssignment>> GetByStatusAsync(InventoryAssignmentStatus status)
    {
        _logger.LogInformation("Получение назначений со статусом: {Status}", status);
        try
        {
            var assignments = await _db.InventoryAssignments
                .Where(a => a.Status == (int)status)
                .ToListAsync();
            return assignments.Select(a => a.ToDomain()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении назначений со статусом: {Status}", status);
            throw;
        }
    }

    public async Task<List<InventoryAssignment>> GetActiveAsync()
    {
        _logger.LogInformation("Получение активных назначений инвентаризации");
        try
        {
            var assignments = await _db.InventoryAssignments
                .Where(a => a.Status != (int)InventoryAssignmentStatus.Completed && a.CompletedAt == null)
                .ToListAsync();
            return assignments.Select(a => a.ToDomain()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении активных назначений");
            throw;
        }
    }

    public async Task<int> AddAsync(InventoryAssignment assignment)
    {
        if (assignment is null)
            throw new ArgumentNullException(nameof(assignment));

        _logger.LogInformation(
            "Создание назначения инвентаризации для пользователя {UserId}, филиал {BranchId}, зона {ZoneCode}",
            assignment.AssignedToUserId, assignment.BranchId, assignment.ZoneCode);
        try
        {
            var model = assignment.ToModel();
            return await _db.InsertAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании назначения инвентаризации");
            throw;
        }
    }

    public async Task<int> UpdateAsync(InventoryAssignment assignment)
    {
        if (assignment is null)
            throw new ArgumentNullException(nameof(assignment));

        _logger.LogInformation(
            "Обновление назначения инвентаризации ID {AssignmentId}, новый статус {Status}",
            assignment.Id, assignment.Status);
        try
        {
            var model = assignment.ToModel();
            return await _db.UpdateAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении назначения ID: {AssignmentId}", assignment.Id);
            throw;
        }
    }
}

public interface IInventoryAssignmentLineRepository
{
    Task<InventoryAssignmentLine> GetByIdAsync(int id);
    Task<List<InventoryAssignmentLine>> GetByAssignmentIdAsync(int inventoryAssignmentId);
    Task<List<InventoryAssignmentLine>> GetByItemPositionIdAsync(int itemPositionId);
    Task<List<InventoryAssignmentLine>> GetUncountedAsync(int inventoryAssignmentId);
    Task<int> AddAsync(InventoryAssignmentLine line);
    Task<int> UpdateAsync(InventoryAssignmentLine line);
    Task<int> AddBatchAsync(List<InventoryAssignmentLine> lines);
}

public class InventoryAssignmentLineRepository : IInventoryAssignmentLineRepository
{
    private readonly ITaskDataConnection _db;
    private readonly ILogger<InventoryAssignmentLineRepository> _logger;

    public InventoryAssignmentLineRepository(
        ITaskDataConnection db,
        ILogger<InventoryAssignmentLineRepository> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger;
    }

    public async Task<InventoryAssignmentLine> GetByIdAsync(int id)
    {
        _logger.LogInformation("Получение строки инвентаризации по ID: {LineId}", id);
        try
        {
            var line = await _db.InventoryAssignmentLines
                .FirstOrDefaultAsync(l => l.Id == id);
            return line?.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении строки по ID: {LineId}", id);
            throw;
        }
    }

    public async Task<List<InventoryAssignmentLine>> GetByAssignmentIdAsync(int inventoryAssignmentId)
    {
        _logger.LogInformation("Получение строк для назначения: {AssignmentId}", inventoryAssignmentId);
        try
        {
            var lines = await _db.InventoryAssignmentLines
                .Where(l => l.InventoryAssignmentId == inventoryAssignmentId)
                .ToListAsync();
            return lines.Select(l => l.ToDomain()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении строк для назначения: {AssignmentId}", inventoryAssignmentId);
            throw;
        }
    }

    public async Task<List<InventoryAssignmentLine>> GetByItemPositionIdAsync(int itemPositionId)
    {
        _logger.LogInformation("Получение строк для позиции товара: {ItemPositionId}", itemPositionId);
        try
        {
            var lines = await _db.InventoryAssignmentLines
                .Where(l => l.ItemPositionId == itemPositionId)
                .ToListAsync();
            return lines.Select(l => l.ToDomain()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении строк для позиции: {ItemPositionId}", itemPositionId);
            throw;
        }
    }

    public async Task<List<InventoryAssignmentLine>> GetUncountedAsync(int inventoryAssignmentId)
    {
        _logger.LogInformation("Получение неучтённых строк для назначения: {AssignmentId}", inventoryAssignmentId);
        try
        {
            var lines = await _db.InventoryAssignmentLines
                .Where(l => l.InventoryAssignmentId == inventoryAssignmentId && l.ActualQuantity == null)
                .ToListAsync();
            return lines.Select(l => l.ToDomain()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении неучтённых строк для назначения: {AssignmentId}", inventoryAssignmentId);
            throw;
        }
    }

    public async Task<int> AddAsync(InventoryAssignmentLine line)
    {
        if (line is null)
            throw new ArgumentNullException(nameof(line));

        _logger.LogInformation(
            "Создание строки инвентаризации для назначения {AssignmentId}, товар {ItemPositionId}",
            line.InventoryAssignmentId, line.ItemPositionId);
        try
        {
            var model = line.ToModel();
            return await _db.InsertAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании строки инвентаризации");
            throw;
        }
    }

    public async Task<int> UpdateAsync(InventoryAssignmentLine line)
    {
        if (line is null)
            throw new ArgumentNullException(nameof(line));

        _logger.LogInformation(
            "Обновление строки инвентаризации ID {LineId}: ожидаемо {Expected}, фактически {Actual}",
            line.Id, line.ExpectedQuantity, line.ActualQuantity);
        try
        {
            var model = line.ToModel();
            return await _db.UpdateAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении строки ID: {LineId}", line.Id);
            throw;
        }
    }

    public async Task<int> AddBatchAsync(List<InventoryAssignmentLine> lines)
    {
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("Список строк не может быть пустым");

        _logger.LogInformation("Массовое добавление {Count} строк инвентаризации", lines.Count);
        try
        {
            var models = lines.Select(l => l.ToModel()).ToList();
            return await _db.InsertAsync(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при массовом добавлении строк инвентаризации");
            throw;
        }
    }
}
