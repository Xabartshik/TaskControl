using System;
using System.Collections.Generic;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// Корневой DTO задачи инвентаризации
    /// </summary>
    public class WorkerInventoryTaskDto
    {
        public int AssignmentId { get; set; }
        public int TaskId { get; set; }
        public string? TaskNumber { get; set; }
        public AssignmentStatus Status { get; set; }
        public DateTime? CreatedDate { get; set; }

        // Прогресс
        public int TotalLines { get; set; }
        public int CountedLines { get; set; }

        /// <summary>
        /// Список складских ячеек, которые нужно проверить (группировка).
        /// </summary>
        public List<CellInventoryInfoDto> CellInventories { get; set; } = new();
    }

    /// <summary>
    /// Краткая информация о назначенной инвентаризации (заголовок).
    /// Используется для отображения списка активных/доступных задач работника.
    /// </summary>
    public class InventoryAssignmentHeaderDto
    {
        public int AssignmentId { get; set; }

        public int TaskId { get; set; }

        public AssignmentStatus Status { get; set; }

        public DateTime AssignedAt { get; set; }

        // Поля прогресса выполнения
        public int TotalLines { get; set; }
        public int CountedLines { get; set; }

        /// <summary>
        /// Вычисляемое свойство для удобного отображения прогресс-бара в UI (0-100%)
        /// </summary>
        public double CompletionPercentage =>
            TotalLines == 0 ? 0 : Math.Round((double)CountedLines / TotalLines * 100, 1);
    }

    /// <summary>
    /// Информация о конкретной ячейке, которую нужно проверить
    /// </summary>
    public class CellInventoryInfoDto
    {
        public int PositionId { get; set; }
        public string? CellCode { get; set; } // Полный код, например "A-1-1-1"
        public string? CellDisplayName { get; set; } // Название полки

        /// <summary>
        /// Список товаров, ожидаемых в этой ячейке.
        /// </summary>
        public List<InventoryLineDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Конкретный товар для пересчета в ячейке
    /// </summary>
    public class InventoryLineDto
    {
        public int LineId { get; set; }
        public int ItemPositionId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? Barcode { get; set; }

        public int ExpectedQuantity { get; set; }
        public int? ActualQuantity { get; set; }

        public bool IsCounted => ActualQuantity.HasValue;
    }
}