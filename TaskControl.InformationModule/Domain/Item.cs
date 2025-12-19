using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

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
        /// Название товара
        /// </summary>
        [Required(ErrorMessage = "Название товара обязательно для заполнения")]
        [StringLength(100, ErrorMessage = "Название товара не может превышать 100 символов")]
        public string Name { get; set; }
        /// <summary>
        /// Вес товара в граммах (положительное число)
        /// </summary>
        [Required(ErrorMessage = "Вес товара обязателен для заполнения")]
        public Mass Weight { get; set; }

        /// <summary>
        /// Длина товара в мм (положительное число)
        /// </summary>
        [Required(ErrorMessage = "Длина товара обязательна для заполнения")]
        public Length Length { get; set; }

        /// <summary>
        /// Ширина товара в мм (положительное число)
        /// </summary>
        [Required(ErrorMessage = "Ширина товара обязательна для заполнения")]
        public Length Width { get; set; }

        /// <summary>
        /// Высота товара в мм (положительное число)
        /// </summary>
        [Required(ErrorMessage = "Высота товара обязательна для заполнения")]
        public Length Height { get; set; }

    }
}
