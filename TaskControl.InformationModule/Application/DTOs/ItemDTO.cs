using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.Application.DTOs
{
    /// <summary>
    /// Данные товара для передачи между слоями
    /// </summary>
    public record ItemDto
    {
        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public int ItemId { get; init; }

        /// <summary>
        /// Вес в килограммах
        /// </summary>
        [Required(ErrorMessage = "Укажите вес товара")]
        [Range(0.01, 1000, ErrorMessage = "Вес должен быть от 0.01 до 1000 кг")]
        public decimal Weight { get; init; }

        /// <summary>
        /// Длина в сантиметрах
        /// </summary>
        [Required(ErrorMessage = "Укажите длину товара")]
        [Range(0.1, 500, ErrorMessage = "Длина должна быть от 0.1 до 500 см")]
        public decimal Length { get; init; }

        /// <summary>
        /// Ширина в сантиметрах
        /// </summary>
        [Required(ErrorMessage = "Укажите ширину товара")]
        [Range(0.1, 500, ErrorMessage = "Ширина должна быть от 0.1 до 500 см")]
        public decimal Width { get; init; }

        /// <summary>
        /// Высота в сантиметрах
        /// </summary>
        [Required(ErrorMessage = "Укажите высоту товара")]
        [Range(0.1, 500, ErrorMessage = "Высота должна быть от 0.1 до 500 см")]
        public decimal Height { get; init; }

        /// <summary>
        /// Преобразует сущность в DTO
        /// </summary>
        public static ItemDto ToDto(Item entity) => new()
        {
            ItemId = entity.ItemId,
            Weight = entity.Weight,
            Length = entity.Length,
            Width = entity.Width,
            Height = entity.Height
        };

        /// <summary>
        /// Создает сущность из DTO
        /// </summary>
        public static Item FromDto(ItemDto dto) => new()
        {
            ItemId = dto.ItemId,
            Weight = dto.Weight,
            Length = dto.Length,
            Width = dto.Width,
            Height = dto.Height
        };
    }
}
