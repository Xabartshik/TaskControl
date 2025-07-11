using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskControl.ReportsModule.Domain
{
    /// <summary>
    /// Сырое событие системы для последующей обработки
    /// </summary>
    public class RawEvent
    {
        /// <summary>
        /// Уникальный идентификатор отчета/события
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// Тип события (классификатор)
        /// </summary>
        [Required(ErrorMessage = "Тип события обязателен")]
        [StringLength(50, ErrorMessage = "Тип события не может превышать 50 символов")]
        public string Type { get; set; }

        /// <summary>
        /// Параметры события в формате JSON
        /// </summary>
        [Required(ErrorMessage = "Параметры события обязательны")]
        public JsonDocument JSONParams { get; set; }

        /// <summary>
        /// Время наступления события
        /// </summary>
        [Required(ErrorMessage = "Время события обязательно")]
        public DateTime EventTime { get; set; }

        /// <summary>
        /// Сервис-источник события
        /// </summary>
        [Required(ErrorMessage = "Источник события обязателен")]
        [StringLength(100, ErrorMessage = "Имя сервиса не может превышать 100 символов")]
        public string SourceService { get; set; }
    }
}
