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

    public async Task<InventoryAssignment?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Получение назначения инвентаризации по ID: {AssignmentId}", id);
        try
        {
            var assignment = await _db.InventoryAssignments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                _logger.LogWarning("Назначение с ID {AssignmentId} не найдено", id);
                return null;
            }

            var lines = await _db.InventoryAssignmentLines
                .Where(l => l.InventoryAssignmentId == id)
                .ToListAsync();

            if (lines.Count == 0)
            {
                _logger.LogWarning("У назначения {AssignmentId} нет линий, пропускаем", id);
                return null;
            }

            var domainLines = lines.Select(l => l.ToDomain()).ToList();
            var domain = assignment.ToDomainWithLines(domainLines);

            _logger.LogInformation("Назначение ID {AssignmentId} загружено с {LineCount} линиями",
                id, domainLines.Count);

            return domain;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении назначения по ID: {AssignmentId}", id);
            throw;
        }
    }

    public async Task<InventoryAssignment?> GetByTaskIdAsync(int taskId)
    {
        _logger.LogInformation("Получение назначения инвентаризации по TaskID: {TaskId}", taskId);
        try
        {
            var assignment = await _db.InventoryAssignments
                .FirstOrDefaultAsync(a => a.TaskId == taskId);

            if (assignment == null)
            {
                _logger.LogWarning("Назначение с TaskID {TaskId} не найдено", taskId);
                return null;
            }

            var lines = await _db.InventoryAssignmentLines
                .Where(l => l.InventoryAssignmentId == assignment.Id)
                .ToListAsync();

            if (lines.Count == 0)
            {
                _logger.LogWarning("Assignment для TaskID {TaskId} не имеет линий, skipping", taskId);
                return null;
            }

            var domainLines = lines.Select(l => l.ToDomain()).ToList();
            var domain = assignment.ToDomainWithLines(domainLines);

            _logger.LogInformation("Назначение для TaskID {TaskId} загружено с {LineCount} линиями",
                taskId, domainLines.Count);

            return domain;
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

            var result = new List<InventoryAssignment>();

            foreach (var model in assignments)
            {
                var lines = await _db.InventoryAssignmentLines
                    .Where(l => l.InventoryAssignmentId == model.Id)
                    .ToListAsync();

                // Пропускаем assignment'ы БЕЗ строк (они невалидны по домену)
                if (lines.Count == 0)
                {
                    _logger.LogWarning("У назначения {AssignmentId} нет линий, пропускаем", model.Id);
                    continue;
                }

                var domainLines = lines.Select(l => l.ToDomain()).ToList();
                var domain = model.ToDomainWithLines(domainLines);
                result.Add(domain);
            }

            return result;
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
            model.Id = 0;
            var newId = await _db.InsertWithInt32IdentityAsync(model);
            return newId;
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
            model.Id = 0;
            var newId = await _db.InsertWithInt32IdentityAsync(model);
            return newId;
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

            foreach (var m in models)
                m.Id = 0;

            var inserted = 0;
            foreach (var m in models)
                inserted += await _db.InsertWithInt32IdentityAsync(m); // InsertAsync возвращает int [file:30]

            return inserted; // обычно будет = models.Count
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при массовом добавлении строк инвентаризации");
            throw;
        }
    }

}
