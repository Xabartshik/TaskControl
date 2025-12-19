using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.Domain;
using UnitsNet;

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
        /// Название товара
        /// </summary>
        [Required(ErrorMessage = "Название товара обязательно для заполнения")]
        [StringLength(100, ErrorMessage = "Название товара не может превышать 100 символов")]
        public string Name { get; set; }

        /// <summary>
        /// Вес в граммах
        /// </summary>
        [Required(ErrorMessage = "Укажите вес товара")]
        public Mass Weight { get; init; }

        /// <summary>
        /// Длина в миллиметрах
        /// </summary>
        [Required(ErrorMessage = "Укажите длину товара")]
        public Length Length { get; init; }

        /// <summary>
        /// Ширина в миллиметрах
        /// </summary>
        [Required(ErrorMessage = "Укажите ширину товара")]
        public Length Width { get; init; }

        /// <summary>
        /// Высота в миллиметрах
        /// </summary>
        [Required(ErrorMessage = "Укажите высоту товара")]
        public Length Height { get; init; }

        /// <summary>
        /// Преобразует сущность в DTO
        /// </summary>
        public static ItemDto ToDto(Item entity) => new()
        {
            ItemId = entity.ItemId,
            Name = entity.Name,
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
            Name = dto.Name,
            Weight = dto.Weight,
            Length = dto.Length,
            Width = dto.Width,
            Height = dto.Height
        };
    }
}
