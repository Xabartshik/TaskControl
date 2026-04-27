using System.ComponentModel.DataAnnotations;
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
        public double Weight { get; init; }

        /// <summary>
        /// Длина в миллиметрах
        /// </summary>
        [Required(ErrorMessage = "Укажите длину товара")]
        public double Length { get; init; }

        /// <summary>
        /// Ширина в миллиметрах
        /// </summary>
        [Required(ErrorMessage = "Укажите ширину товара")]
        public double Width { get; init; }

        /// <summary>
        /// Высота в миллиметрах
        /// </summary>
        [Required(ErrorMessage = "Укажите высоту товара")]
        public double Height { get; init; }

        /// <summary>
        /// Преобразует сущность в DTO
        /// </summary>
        public static ItemDto ToDto(Item entity) => new()
        {
            ItemId = entity.ItemId,
            Name = entity.Name,
            Weight = entity.Weight.Grams,
            Length = entity.Length.Millimeters,
            Width = entity.Width.Millimeters,
            Height = entity.Height.Millimeters
        };

        /// <summary>
        /// Создает сущность из DTO
        /// </summary>
        public static Item FromDto(ItemDto dto) => new()
        {
            ItemId = dto.ItemId,
            Name = dto.Name,
            Weight = Mass.FromGrams(dto.Weight),
            Length = UnitsNet.Length.FromMillimeters(dto.Length),
            Width = UnitsNet.Length.FromMillimeters(dto.Width),
            Height = UnitsNet.Length.FromMillimeters(dto.Height)
        };
    }
}
