using System.Threading.Tasks;
using TaskControl.InventoryModule.Domain;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Interface;

public interface IInventoryStatisticsRepository
{
    /// <summary>
    /// Получить статистику по ID
    /// </summary>
    Task<InventoryStatistics> GetByIdAsync(int id);

    /// <summary>
    /// Получить статистику по ID назначения инвентаризации
    /// </summary>
    Task<InventoryStatistics> GetByAssignmentIdAsync(int inventoryAssignmentId);

    /// <summary>
    /// Создать новую статистику
    /// </summary>
    Task<int> AddAsync(InventoryStatistics statistics);

    /// <summary>
    /// Обновить статистику
    /// </summary>
    Task<int> UpdateAsync(InventoryStatistics statistics);
}
