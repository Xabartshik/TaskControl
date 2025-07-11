using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InformationModule.Domain
{
    /// <summary>
    /// Филиал компании
    /// </summary>
    public class Branch
    {
        /// <summary>
        /// Уникальный идентификатор филиала
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// Название филиала (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Название филиала обязательно для заполнения")]
        [StringLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        public string BranchName { get; set; }

        /// <summary>
        /// Тип филиала (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Тип филиала обязателен для заполнения")]
        [StringLength(100, ErrorMessage = "Тип филиала не может превышать 100 символов")]
        public string BranchType { get; set; }

        /// <summary>
        /// Адрес филиала (обязательное поле)
        /// </summary>
        [Required(ErrorMessage = "Адрес филиала обязателен для заполнения")]
        [StringLength(500, ErrorMessage = "Адрес не может превышать 500 символов")]
        public string Address { get; set; }

        /// <summary>
        /// Дата создания (устанавливается автоматически)
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата обновления (устанавливается автоматически)
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

