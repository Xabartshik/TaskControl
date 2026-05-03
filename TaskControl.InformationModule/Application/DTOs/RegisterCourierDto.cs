using System.ComponentModel.DataAnnotations;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.Application.DTOs
{
    public class RegisterCourierDto
    {
        // --- Данные сотрудника ---
        [Required]
        public string Surname { get; set; }
        [Required]
        public string Name { get; set; }
        public string? MiddleName { get; set; }

        // --- Данные машины курьера ---
        [Required]
        public int VehicleTypeId { get; set; } // Например: 1 - Пеший, 2 - Легковая, 3 - Фургон

        [Required]
        public double MaxWeightGrams { get; set; } // Например: 100000 (100 кг)

        [Required]
        public double MaxLengthMm { get; set; }

        [Required]
        public double MaxWidthMm { get; set; }

        [Required]
        public double MaxHeightMm { get; set; }
    }
}