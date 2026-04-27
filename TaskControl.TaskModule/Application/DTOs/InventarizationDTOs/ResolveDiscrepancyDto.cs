using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.DTOs.InventorizationDTOs
{
    /// <summary>
    /// DTO для разрешения расхождения (отметить как решённое, в расследовании и т.д.)
    /// </summary>
    public class ResolveDiscrepancyDto
    {
        [Required(ErrorMessage = "ID расхождения обязателен")]
        public int DiscrepancyId { get; set; }

        [Required(ErrorMessage = "Статус разрешения обязателен")]
        public DiscrepancyResolutionStatus ResolutionStatus { get; set; }

        [StringLength(500, ErrorMessage = "Причина не может быть больше 500 символов")]
        public string? Reason { get; set; }
    }
}
