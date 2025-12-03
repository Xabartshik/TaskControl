namespace TaskControl.TaskModule.Domain;

/// <summary>
/// Статистика по выполненной инвентаризации.
/// Агрегирует данные о ходе выполнения инвентаризационного назначения.
/// </summary>
public class InventoryStatistics
{
    /// <summary>
    /// Уникальный идентификатор статистики
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID инвентаризационного назначения (InventoryAssignment)
    /// </summary>
    public int InventoryAssignmentId { get; set; }

    /// <summary>
    /// Общее количество позиций к инвентаризации
    /// </summary>
    public int TotalPositions { get; set; }

    /// <summary>
    /// Количество учтённых позиций (с установленным ActualQuantity)
    /// </summary>
    public int CountedPositions { get; set; }

    /// <summary>
    /// Количество позиций, требующих внимания (расхождения)
    /// </summary>
    public int DiscrepancyCount { get; set; }

    /// <summary>
    /// Количество позиций с излишком
    /// </summary>
    public int SurplusCount { get; set; }

    /// <summary>
    /// Количество позиций с недостачей
    /// </summary>
    public int ShortageCount { get; set; }

    /// <summary>
    /// Общая величина излишка (количество товара)
    /// </summary>
    public int TotalSurplusQuantity { get; set; }

    /// <summary>
    /// Общая величина недостачи (количество товара)
    /// </summary>
    public int TotalShortageQuantity { get; set; }

    /// <summary>
    /// Процент завершённости инвентаризации (0-100)
    /// </summary>
    public decimal CompletionPercentage => TotalPositions > 0
        ? decimal.Round((decimal)CountedPositions / TotalPositions * 100, 2)
        : 0;

    /// <summary>
    /// Процент позиций с расхождениями (0-100)
    /// </summary>
    public decimal DiscrepancyPercentage => TotalPositions > 0
        ? decimal.Round((decimal)DiscrepancyCount / TotalPositions * 100, 2)
        : 0;

    /// <summary>
    /// Время начала инвентаризации
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Время завершения инвентаризации
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Продолжительность инвентаризации
    /// </summary>
    public TimeSpan Duration => 
        CompletedAt.HasValue 
            ? CompletedAt.Value - StartedAt
            : DateTime.UtcNow - StartedAt;

    public InventoryStatistics()
    {
    }

    public InventoryStatistics(int inventoryAssignmentId, int totalPositions)
    {
        if (inventoryAssignmentId <= 0)
            throw new ArgumentOutOfRangeException(nameof(inventoryAssignmentId), "InventoryAssignmentId должен быть положительным");

        if (totalPositions <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalPositions), "TotalPositions должно быть положительным");

        InventoryAssignmentId = inventoryAssignmentId;
        TotalPositions = totalPositions;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновить статистику на основе результатов инвентаризации
    /// </summary>
    public void Update(
        int countedPositions,
        int discrepancyCount,
        int surplusCount,
        int shortageCount,
        int totalSurplusQuantity,
        int totalShortageQuantity)
    {
        if (countedPositions < 0 || countedPositions > TotalPositions)
            throw new ArgumentOutOfRangeException(nameof(countedPositions), "CountedPositions должно быть между 0 и TotalPositions");

        if (discrepancyCount < 0)
            throw new ArgumentOutOfRangeException(nameof(discrepancyCount), "DiscrepancyCount не может быть отрицательным");

        if (surplusCount < 0 || shortageCount < 0)
            throw new ArgumentOutOfRangeException(nameof(surplusCount), "Counts не могут быть отрицательными");

        CountedPositions = countedPositions;
        DiscrepancyCount = discrepancyCount;
        SurplusCount = surplusCount;
        ShortageCount = shortageCount;
        TotalSurplusQuantity = totalSurplusQuantity;
        TotalShortageQuantity = totalShortageQuantity;
    }

    /// <summary>
    /// Отметить инвентаризацию как завершённую
    /// </summary>
    public void Complete()
    {
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Проверить, завершена ли инвентаризация
    /// </summary>
    public bool IsCompleted => CompletedAt.HasValue;

    /// <summary>
    /// Получить строковое представление статистики
    /// </summary>
    public override string ToString()
    {
        return $"Инвентаризация {InventoryAssignmentId}: " +
               $"Учтено {CountedPositions}/{TotalPositions} ({CompletionPercentage}%), " +
               $"Расхождений: {DiscrepancyCount} (Излишек: {SurplusCount}, Недостача: {ShortageCount})";
    }
}
