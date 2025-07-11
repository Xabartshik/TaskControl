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
    /// Data Transfer Object для сущности Branch
    /// </summary>
    public record BranchDto
    {
        /// <summary>
        /// Уникальный идентификатор филиала
        /// </summary>
        public int BranchId { get; init; }

        /// <summary>
        /// Название филиала
        /// </summary>
        [Required(ErrorMessage = "Название филиала обязательно для заполнения")]
        [StringLength(200, ErrorMessage = "Название филиала не может превышать 200 символов")]
        public string BranchName { get; init; }

        /// <summary>
        /// Тип филиала
        /// </summary>
        [Required(ErrorMessage = "Тип филиала обязателен для заполнения")]
        [StringLength(100, ErrorMessage = "Тип филиала не может превышать 100 символов")]
        public string BranchType { get; init; }

        /// <summary>
        /// Адрес филиала
        /// </summary>
        [Required(ErrorMessage = "Адрес филиала обязателен для заполнения")]
        [StringLength(500, ErrorMessage = "Адрес филиала не может превышать 500 символов")]
        public string Address { get; init; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// Дата последнего обновления записи
        /// </summary>
        public DateTime UpdatedAt { get; init; }

        /// <summary>
        /// Преобразует сущность Branch в BranchDto
        /// </summary>
        public static BranchDto ToDto(Branch entity)
        {
            return new BranchDto
            {
                BranchId = entity.BranchId,
                BranchName = entity.BranchName,
                BranchType = entity.BranchType,
                Address = entity.Address,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        /// <summary>
        /// Создает сущность Branch из BranchDto
        /// </summary>
        public static Branch FromDto(BranchDto dto)
        {
            return new Branch
            {
                BranchId = dto.BranchId,
                BranchName = dto.BranchName,
                BranchType = dto.BranchType,
                Address = dto.Address
                // CreatedAt и UpdatedAt будут установлены автоматически
            };
        }
    }
}
