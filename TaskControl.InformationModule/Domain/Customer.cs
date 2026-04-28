using System;
using System.ComponentModel.DataAnnotations;

namespace TaskControl.InformationModule.Domain
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Фамилия обязательна")]
        public string LastName { get; set; } = null!;

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}