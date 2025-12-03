namespace TaskControl.TaskModule.Domain;

/// <summary>
/// Расхождение между ожидаемым и фактическим количеством товара.
/// Используется для анализа результатов инвентаризации.
/// </summary>
public class InventoryDiscrepancy
{
    /// <summary>
    /// Уникальный идентификатор расхождения
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID строки назначения инвентаризации (InventoryAssignmentLine)
    /// </summary>
    public int InventoryAssignmentLineId { get; set; }

    /// <summary>
    /// ID товара на позиции (ItemPosition из OrderModule)
    /// </summary>
    public int ItemPositionId { get; set; }

    /// <summary>
    /// Ожидаемое количество
    /// </summary>
    public int ExpectedQuantity { get; set; }

    /// <summary>
    /// Фактическое количество (отсчитано)
    /// </summary>
    public int ActualQuantity { get; set; }

    /// <summary>
    /// Разница (может быть отрицательной). ActualQuantity - ExpectedQuantity
    /// </summary>
    public int Variance => ActualQuantity - ExpectedQuantity;

    /// <summary>
    /// Тип расхождения: излишек, недостача, отсутствие
    /// </summary>
    public DiscrepancyType Type { get; set; }

    /// <summary>
    /// Примечание/комментарий
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// Дата выявления расхождения
    /// </summary>
    public DateTime IdentifiedAt { get; set; }

    /// <summary>
    /// Статус решения расхождения
    /// </summary>
    public DiscrepancyResolutionStatus ResolutionStatus { get; set; }

    public InventoryDiscrepancy()
    {
    }

    public InventoryDiscrepancy(
        int inventoryAssignmentLineId,
        int itemPositionId,
        int expectedQuantity,
        int actualQuantity,
        string note = "")
    {
        if (inventoryAssignmentLineId <= 0)
            throw new ArgumentOutOfRangeException(nameof(inventoryAssignmentLineId), "InventoryAssignmentLineId должен быть положительным");

        if (itemPositionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(itemPositionId), "ItemPositionId должен быть положительным");

        if (expectedQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedQuantity), "ExpectedQuantity не может быть отрицательным");

        if (actualQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(actualQuantity), "ActualQuantity не может быть отрицательным");

        InventoryAssignmentLineId = inventoryAssignmentLineId;
        ItemPositionId = itemPositionId;
        ExpectedQuantity = expectedQuantity;
        ActualQuantity = actualQuantity;
        Note = note ?? "";
        IdentifiedAt = DateTime.UtcNow;
        ResolutionStatus = DiscrepancyResolutionStatus.Pending;

        // Определить тип расхождения
        Type = Variance switch
        {
            > 0 => DiscrepancyType.Surplus,     // Излишек
            < 0 => DiscrepancyType.Shortage,    // Недостача
            _ => DiscrepancyType.None           // Нет расхождения
        };
    }

    /// <summary>
    /// Добавить комментарий к расхождению
    /// </summary>
    public void AddNote(string additionalNote)
    {
        if (!string.IsNullOrWhiteSpace(additionalNote))
            Note += string.IsNullOrEmpty(Note) ? additionalNote : $" | {additionalNote}";
    }

    /// <summary>
    /// Отметить расхождение как разрешённое
    /// </summary>
    public void Resolve(string reason = "")
    {
        ResolutionStatus = DiscrepancyResolutionStatus.Resolved;
        if (!string.IsNullOrWhiteSpace(reason))
            AddNote($"[RESOLVED] {reason}");
    }

    /// <summary>
    /// Отметить расхождение как требующее расследования
    /// </summary>
    public void MarkForInvestigation(string reason = "")
    {
        ResolutionStatus = DiscrepancyResolutionStatus.UnderInvestigation;
        if (!string.IsNullOrWhiteSpace(reason))
            AddNote($"[INVESTIGATION] {reason}");
    }

    /// <summary>
    /// Отметить как списано в убыль
    /// </summary>
    public void MarkAsWrittenOff(string reason = "")
    {
        ResolutionStatus = DiscrepancyResolutionStatus.WrittenOff;
        if (!string.IsNullOrWhiteSpace(reason))
            AddNote($"[WRITTEN OFF] {reason}");
    }

    /// <summary>
    /// Получить строковое представление расхождения
    /// </summary>
    public override string ToString()
    {
        return $"Расхождение {Id}: " +
               $"Ожидаемо {ExpectedQuantity}, Фактически {ActualQuantity} " +
               $"(Разница: {Variance:+#;-#;0}), Тип: {Type}, Статус: {ResolutionStatus}";
    }
}

/// <summary>
/// Тип расхождения при инвентаризации
/// </summary>
public enum DiscrepancyType
{
    /// <summary>
    /// Нет расхождения
    /// </summary>
    None = 0,

    /// <summary>
    /// Излишек (фактическое > ожидаемого)
    /// </summary>
    Surplus = 1,

    /// <summary>
    /// Недостача (фактическое < ожидаемого)
    /// </summary>
    Shortage = 2
}

/// <summary>
/// Статус решения расхождения
/// </summary>
public enum DiscrepancyResolutionStatus
{
    /// <summary>
    /// В ожидании
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Разрешено
    /// </summary>
    Resolved = 1,

    /// <summary>
    /// Требует расследования
    /// </summary>
    UnderInvestigation = 2,

    /// <summary>
    /// Написано в убыль
    /// </summary>
    WrittenOff = 3
}
