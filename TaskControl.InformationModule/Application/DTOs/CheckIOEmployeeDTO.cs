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
    /// Данные отметки прихода/ухода сотрудника
    /// </summary>
    public record CheckIOEmployeeDto
    {
        /// <summary>
        /// Идентификатор записи
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Идентификатор сотрудника
        /// </summary>
        [Required(ErrorMessage = "Укажите ID сотрудника")]
        public int EmployeeId { get; init; }

        /// <summary>
        /// Идентификатор филиала
        /// </summary>
        [Required(ErrorMessage = "Укажите ID филиала")]
        public int BranchId { get; init; }

        /// <summary>
        /// Тип отметки: "in" - вход, "out" - выход
        /// </summary>
        [Required(ErrorMessage = "Укажите тип отметки (in/out)")]
        [RegularExpression("^(in|out)$", ErrorMessage = "Допустимые значения: 'in' или 'out'")]
        public string CheckType { get; init; }

        /// <summary>
        /// Дата и время отметки
        /// </summary>
        public DateTime CheckTimeStamp { get; init; }

        /// <summary>
        /// Преобразует сущность в DTO
        /// </summary>
        public static CheckIOEmployeeDto ToDto(CheckIOEmployee entity) => new()
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            BranchId = entity.BranchId,
            CheckType = entity.CheckType,
            CheckTimeStamp = entity.CheckTimeStamp
        };

        /// <summary>
        /// Создает сущность из DTO
        /// </summary>
        public static CheckIOEmployee FromDto(CheckIOEmployeeDto dto) => new()
        {
            Id = dto.Id,
            EmployeeId = dto.EmployeeId,
            BranchId = dto.BranchId,
            CheckType = dto.CheckType,
            CheckTimeStamp = dto.CheckTimeStamp
        };
    }
}
