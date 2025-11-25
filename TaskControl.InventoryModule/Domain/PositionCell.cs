using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace TaskControl.InventoryModule.Domain
{
    /// <summary>
    /// Человекочитаемый номер/адрес складской позиции.
    /// </summary>
    public class PositionCode
    {
        /// <summary>
        /// Идентификатор филиала.
        /// </summary>
        [Required(ErrorMessage = "Филиал обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID филиала")]
        public int BranchId { get; set; }

        /// <summary>
        /// Код зоны хранения.
        /// </summary>
        [Required(ErrorMessage = "Код зоны хранения обязателен")]
        [StringLength(10, ErrorMessage = "Код зоны не должен превышать 10 символов")]
        public string ZoneCode { get; set; }

        /// <summary>
        /// Тип хранилища первого уровня (стеллаж, пол, ячейка и т.п.).
        /// </summary>
        [Required(ErrorMessage = "Тип хранилища первого уровня обязателен")]
        [StringLength(30, ErrorMessage = "Тип не должен превышать 30 символов")]
        public string FirstLevelStorageType { get; set; }

        /// <summary>
        /// Номер хранилища первого уровня.
        /// </summary>
        [Required(ErrorMessage = "Номер хранилища первого уровня обязателен")]
        [StringLength(20, ErrorMessage = "Номер хранилища первого уровня не должен превышать 20 символов")]
        public string FLSNumber { get; set; }

        /// <summary>
        /// Номер хранилища второго уровня (опционально).
        /// </summary>
        [StringLength(30, ErrorMessage = "Значение не должно превышать 30 символов")]
        public string? SecondLevelStorage { get; set; }

        /// <summary>
        /// Номер хранилища третьего уровня (опционально).
        /// </summary>
        [StringLength(30, ErrorMessage = "Значение не должно превышать 30 символов")]
        public string? ThirdLevelStorage { get; set; }

        public override string ToString()
        {
            // Пример: BR1-ZA-RACK-A1-S1-C3
            return $"{BranchId}-{ZoneCode}-{FirstLevelStorageType}-{FLSNumber}" +
                   (SecondLevelStorage is { Length: > 0 } ? $"-{SecondLevelStorage}" : string.Empty) +
                   (ThirdLevelStorage is { Length: > 0 } ? $"-{ThirdLevelStorage}" : string.Empty);
        }

        // Пример ожидаемого формата:
        // "1-ZA-RACK-A1-S1-C3"
        public static PositionCode FromString(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Код позиции не может быть пустым.", nameof(code));

            var parts = code.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Минимум: BranchId, ZoneCode, FirstLevelStorageType, FLSNumber
            if (parts.Length < 4)
                throw new FormatException("Некорректный формат кода позиции. Ожидается минимум 4 части.");

            if (!int.TryParse(parts[0], out var branchId) || branchId <= 0)
                throw new FormatException("Некорректный BranchId в коде позиции.");

            var result = new PositionCode
            {
                BranchId = branchId,
                ZoneCode = parts[1],
                FirstLevelStorageType = parts[2],
                FLSNumber = parts[3]
            };

            // Опциональные уровни хранения
            if (parts.Length >= 5)
                result.SecondLevelStorage = parts[4];

            if (parts.Length >= 6)
                result.ThirdLevelStorage = parts[5];

            return result;
        }
    }

    /// <summary>
    /// Складская позиция/ячейка.
    /// </summary>
    public class PositionCell
    {
        public int PositionId { get; set; }

        /// <summary>
        /// Номер/адрес позиции (ветка склада).
        /// Инкапсулирует BranchId, ZoneCode, уровни хранения.
        /// </summary>
        [Required]
        public PositionCode Code { get; set; } = new();

        //TODO: Добавить ячейке больше статусов, например, зарезервировано

        /// <summary>
        /// Статус ячейки.
        /// </summary>
        [Required]
        [RegularExpression("^(Active|Inactive|Maintenance)$",
            ErrorMessage = "Допустимые статусы: Active, Inactive, Maintenance")]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Длина ячейки в миллиметрах.
        /// </summary>
        public Length Length { get; set; }

        /// <summary>
        /// Ширина ячейки в миллиметрах.
        /// </summary>
        public Length Width { get; set; }

        /// <summary>
        /// Высота ячейки в миллиметрах.
        /// </summary>
        public Length Height { get; set; }

        /// <summary>
        /// Проверяет доступность позиции.
        /// </summary>
        public bool IsAvailable() => Status == "Active";
    }
}
