using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Domain;

namespace TaskControl.ReportsModule.Application
{
    /// <summary>
    /// DTO для передачи сырых событий
    /// </summary>
    public record RawEventDto
    {
        /// <summary>
        /// Идентификатор отчета
        /// </summary>
        [Required(ErrorMessage = "Укажите идентификатор отчета")]
        public int ReportId { get; init; }

        /// <summary>
        /// Тип события
        /// </summary>
        [Required(ErrorMessage = "Укажите тип события")]
        [StringLength(50, ErrorMessage = "Тип события не должен превышать 50 символов")]
        public string Type { get; init; }

        /// <summary>
        /// JSON-параметры (валидируется как строка)
        /// </summary>
        [Required(ErrorMessage = "Параметры события обязательны")]
        [RegularExpression(@"^\s*\{.*\}\s*$", ErrorMessage = "Не валидный JSON-формат")]
        public string JSONParams { get; init; }

        /// <summary>
        /// Время события
        /// </summary>
        [Required(ErrorMessage = "Укажите время события")]
        public DateTime EventTime { get; init; }

        /// <summary>
        /// Сервис-источник
        /// </summary>
        [Required(ErrorMessage = "Укажите сервис-источник")]
        [StringLength(100, ErrorMessage = "Название сервиса не должно превышать 100 символов")]
        public string SourceService { get; init; }

        /// <summary>
        /// Преобразует DTO в сущность
        /// </summary>
        public static RawEvent FromDto(RawEventDto dto)
        {
            return new RawEvent
            {
                ReportId = dto.ReportId,
                Type = dto.Type,
                JSONParams = JsonDocument.Parse(dto.JSONParams),
                EventTime = dto.EventTime,
                SourceService = dto.SourceService
            };
        }

        /// <summary>
        /// Преобразует сущность в DTO
        /// </summary>
        public static RawEventDto ToDto(RawEvent entity)
        {
            return new RawEventDto
            {
                ReportId = entity.ReportId,
                Type = entity.Type,
                JSONParams = entity.JSONParams.RootElement.ToString(),
                EventTime = entity.EventTime,
                SourceService = entity.SourceService
            };
        }
    }
}
