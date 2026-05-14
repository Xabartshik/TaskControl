public record BulkCreatePositionDto
{
    public int BranchId { get; init; }
    public string ZoneCode { get; init; } // Например, "A"
    public string StorageType { get; init; } // Например, "RACK"

    // Параметры итерации
    public int StartFLSNumber { get; init; } = 1;
    public int StorageCount { get; init; } = 1; // Сколько стеллажей/паллет создать

    // Иерархия (null для паллет)
    public int? ShelvesCount { get; init; }
    public int? CellsPerShelf { get; init; }

    // Размеры по умолчанию для создаваемых ячеек
    public double? DefaultLength { get; init; }
    public double? DefaultWidth { get; init; }
    public double? DefaultHeight { get; init; }
}