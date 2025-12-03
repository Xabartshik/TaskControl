using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InventoryModule.Domain;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Interface;

public interface IInventoryDiscrepancyRepository
{
    /// <summary>
    /// Получить расхождение по ID
    /// </summary>
    Task<InventoryDiscrepancy> GetByIdAsync(int id);

    /// <summary>
    /// Получить все расхождения по строке инвентаризации
    /// </summary>
    Task<List<InventoryDiscrepancy>> GetByAssignmentLineIdAsync(int inventoryAssignmentLineId);

    /// <summary>
    /// Получить все расхождения по назначению инвентаризации
    /// </summary>
    Task<List<InventoryDiscrepancy>> GetByAssignmentIdAsync(int inventoryAssignmentId);

    /// <summary>
    /// Получить все нерешённые расхождения
    /// </summary>
    Task<List<InventoryDiscrepancy>> GetPendingAsync();

    /// <summary>
    /// Получить расхождения по типу (излишек/недостача)
    /// </summary>
    Task<List<InventoryDiscrepancy>> GetByTypeAsync(DiscrepancyType type);

    /// <summary>
    /// Создать новое расхождение
    /// </summary>
    Task<int> CreateAsync(InventoryDiscrepancy discrepancy);

    /// <summary>
    /// Обновить расхождение
    /// </summary>
    Task UpdateAsync(InventoryDiscrepancy discrepancy);

    /// <summary>
    /// Удалить расхождение
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Получить общее количество расхождений по назначению
    /// </summary>
    Task<int> GetCountByAssignmentIdAsync(int inventoryAssignmentId);

    /// <summary>
    /// Получить количество нерешённых расхождений
    /// </summary>
    Task<int> GetPendingCountAsync();
}
