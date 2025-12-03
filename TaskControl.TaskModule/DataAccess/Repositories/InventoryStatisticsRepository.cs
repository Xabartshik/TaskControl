using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Repositories;

public interface IInventoryStatisticsRepository
{
    Task<InventoryStatistics> GetByIdAsync(int id);
    Task<InventoryStatistics> GetByAssignmentIdAsync(int inventoryAssignmentId);
    Task<int> AddAsync(InventoryStatistics statistics);
    Task<int> UpdateAsync(InventoryStatistics statistics);
}

public class InventoryStatisticsRepository : IInventoryStatisticsRepository
{
    private readonly ITaskDataConnection _db;
    private readonly ILogger<InventoryStatisticsRepository> _logger;

    public InventoryStatisticsRepository(
        ITaskDataConnection db,
        ILogger<InventoryStatisticsRepository> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger;
    }

    public async Task<InventoryStatistics> GetByIdAsync(int id)
    {
        _logger.LogInformation("Получение статистики по ID: {StatisticsId}", id);
        try
        {
            var statistics = await _db.InventoryStatistics
                .FirstOrDefaultAsync(s => s.Id == id);
            return statistics?.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики по ID: {StatisticsId}", id);
            throw;
        }
    }

    public async Task<InventoryStatistics> GetByAssignmentIdAsync(int inventoryAssignmentId)
    {
        _logger.LogInformation("Получение статистики для назначения инвентаризации: {AssignmentId}", inventoryAssignmentId);
        try
        {
            var statistics = await _db.InventoryStatistics
                .FirstOrDefaultAsync(s => s.InventoryAssignmentId == inventoryAssignmentId);
            return statistics?.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики для назначения: {AssignmentId}", inventoryAssignmentId);
            throw;
        }
    }

    public async Task<int> AddAsync(InventoryStatistics statistics)
    {
        if (statistics is null)
            throw new ArgumentNullException(nameof(statistics));

        _logger.LogInformation("Создание статистики для назначения: {AssignmentId}, всего позиций: {TotalPositions}",
            statistics.InventoryAssignmentId, statistics.TotalPositions);
        try
        {
            var model = statistics.ToModel();
            return await _db.InsertAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании статистики для назначения: {AssignmentId}",
                statistics.InventoryAssignmentId);
            throw;
        }
    }

    public async Task<int> UpdateAsync(InventoryStatistics statistics)
    {
        if (statistics is null)
            throw new ArgumentNullException(nameof(statistics));

        _logger.LogInformation(
            "Обновление статистики для назначения: {AssignmentId}, учтено: {CountedPositions}/{TotalPositions} ({Percentage}%)",
            statistics.InventoryAssignmentId, statistics.CountedPositions, statistics.TotalPositions,
            statistics.CompletionPercentage);
        try
        {
            var model = statistics.ToModel();
            return await _db.UpdateAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статистики для назначения: {AssignmentId}",
                statistics.InventoryAssignmentId);
            throw;
        }
    }
}
