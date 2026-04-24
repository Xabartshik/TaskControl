using TaskControl.InventoryModule.Domain;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// DTO для детальной информации о назначении инвентаризации
    /// </summary>
    public class InventoryAssignmentDetailedWithItemDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int AssignedToUserId { get; set; }
        public string? UserName { get; set; }
        public int BranchId { get; set; }
        public string? ZoneCode { get; set; }
        public InventoryAssignmentStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<InventoryAssignmentLineWithItemDto> Lines { get; set; } = new();
    }
    /// <summary>
    /// DTO для строки назначения инвентаризации с информацией о товаре
    /// Используется при отправке данных на мобильное приложение
    /// </summary>
    public class InventoryAssignmentLineWithItemDto
    {
        public int Id { get; set; }

        public int InventoryAssignmentId { get; set; }

        public int ItemPositionId { get; set; }

        public int PositionId { get; set; }

        /// <summary> Снимок кода позиции на момент назначения</summary>
        public PositionCode? PositionCode { get; set; }

        public int ExpectedQuantity { get; set; }

        public int? ActualQuantity { get; set; }

        /// <summary>Расхождение между ожиданием и фактом</summary>
        public int Variance =>
            ActualQuantity.HasValue ? ActualQuantity.Value - ExpectedQuantity : 0;

        /// <summary> ID товара (для связи с каталогом)</summary>
        public int ItemId { get; set; }

        /// <summary> Название товара (что сканировать/учитывать)</summary>
        public string? ItemName { get; set; }

        /// <summary>Полный текст для отображения</summary>
        public string DisplayName =>
            !string.IsNullOrWhiteSpace(ItemName)
                ? ItemName
                : $"ItemId {ItemId}";
    }
}