using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace TaskControl.InventoryModule.Domain
{

    /// <summary>
    /// Складская позиция/ячейка
    /// </summary>
    public class PositionCell
    {
        public int PositionId { get; set; }

        /// <summary>
        /// Идентификатор филиала
        /// </summary>
        [Required(ErrorMessage = "Филиал обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID филиала")]
        public int BranchId { get; set; }
        //TODO: Добавить ячейке больше статусов, например, зарезервировано
        /// <summary>
        /// Статус ячейки
        /// </summary>
        [Required]
        [RegularExpression("^(Active|Inactive|Maintenance)$",
            ErrorMessage = "Допустимые статусы: Active, Inactive, Maintenance")]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Код зоны хранения -- некоторой области склада
        /// </summary>
        [StringLength(10, ErrorMessage = "Код зоны не должен превышать 10 символов")]
        public string? ZoneCode { get; set; }
        /// <summary>
        /// Тип хранилища первого уровня -- (какой-то объект мебели или четко выделенная область для хранения) стеллаж, пол, ячейка, витрина, постамат и так далее
        /// </summary>
        [Required(ErrorMessage = "Тип хранилища первого уровня обязателен")]
        [StringLength(30, ErrorMessage = "Тип не должен превышать 30 символов")]
        public string FirstLevelStorageType { get; set; }

        /// <summary>
        /// Номер хранилища первого уровня
        /// </summary>
        [StringLength(20, ErrorMessage = "Номер хранилища первого уровня не должен превышать 20 символов")]
        public string? FLSNumber { get; set; }

        /// <summary>
        /// Номер хранилища второго уровня -- какой-то сектор внутри хранилища первого уровня (полка стеллажа и так далее)
        /// </summary>
        [StringLength(30, ErrorMessage = "Значение не должно превышать 30 символов")]
        public string? SecondLevelStorage { get; set; }

        /// <summary>
        /// Номер хранилища третьего уровня -- какой-то сектор внутри хранилища второго уровня (обычно конечный) (ячейка постомата, полочка в шкафе)
        /// </summary>
        [StringLength(30, ErrorMessage = "Значение не должно превышать 30 символов")]
        public string? ThirdLevelStorage { get; set; }

        /// <summary>
        /// Длина ячейки в метрах
        /// </summary>
        [Range(0.01, 20, ErrorMessage = "Длина должна быть от 0.01 до 20 м")]
        public Length Length { get; set; }
        /// <summary>
        /// Ширина ячейки в метрах
        /// </summary>
        [Range(0.01, 20, ErrorMessage = "Ширина должна быть от 0.01 до 20 м")]
        public Length Width { get; set; }
        /// <summary>
        /// Высота ячейки в метрах
        /// </summary>
        [Range(0.01, 20, ErrorMessage = "Высота должна быть от 0.01 до 20 м")]
        public Length Height { get; set; }


        /// <summary>
        /// Проверяет доступность позиции
        /// </summary>
        public bool IsAvailable() => Status == "Active";
    }
}

