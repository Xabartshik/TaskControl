using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InformationModule.Domain
{
    /// <summary>
    /// Товар/предмет в системе
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Уникальный идентификатор товара
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Вес товара в кг (положительное число)
        /// </summary>
        [Required(ErrorMessage = "Вес товара обязателен для заполнения")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Вес должен быть положительным числом")]
        public decimal Weight { get; set; }

        /// <summary>
        /// Длина товара в см (положительное число)
        /// </summary>
        [Required(ErrorMessage = "Длина товара обязательна для заполнения")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Длина должна быть положительным числом")]
        public decimal Length { get; set; }

        /// <summary>
        /// Ширина товара в см (положительное число)
        /// </summary>
        [Required(ErrorMessage = "Ширина товара обязательна для заполнения")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ширина должна быть положительным числом")]
        public decimal Width { get; set; }

        /// <summary>
        /// Высота товара в см (положительное число)
        /// </summary>
        [Required(ErrorMessage = "Высота товара обязательна для заполнения")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Высота должна быть положительным числом")]
        public decimal Height { get; set; }

        /// <summary>
        /// Рассчитывает объем товара в м³
        /// </summary>
        public decimal CalculateVolume() => (Length * Width * Height) / 1_000_000;
    }
}
