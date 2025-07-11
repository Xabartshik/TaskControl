using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.OrderModule.Domain;

namespace TaskControl.OrderModule.Application.DTOs
{
    /// <summary>
    /// Данные заказа для API
    /// </summary>
    public record OrderDto
    {
        public int OrderId { get; init; }

        [Required(ErrorMessage = "Укажите ID клиента")]
        [Range(1, int.MaxValue, ErrorMessage = "ID клиента должен быть положительным числом")]
        public int CustomerId { get; init; }

        [Required(ErrorMessage = "Укажите ID филиала")]
        [Range(1, int.MaxValue, ErrorMessage = "ID филиала должен быть положительным числом")]
        public int BranchId { get; init; }

        [FutureDate(ErrorMessage = "Дата доставки должна быть в будущем")]
        public DateTime? DeliveryDate { get; init; }

        [Required(ErrorMessage = "Укажите тип заказа")]
        [RegularExpression("^(Online|Offline)$")]
        public string Type { get; init; }

        [Required]
        [RegularExpression("^(New|Processing|Delivered|Cancelled)$")]
        public string Status { get; init; } = "New";

        public static Order FromDto(OrderDto dto) => new()
        {
            OrderId = dto.OrderId,
            CustomerId = dto.CustomerId,
            BranchId = dto.BranchId,
            DeliveryDate = dto.DeliveryDate,
            Type = dto.Type,
            Status = dto.Status
        };

        public static OrderDto ToDto(Order entity) => new()
        {
            OrderId = entity.OrderId,
            CustomerId = entity.CustomerId,
            BranchId = entity.BranchId,
            DeliveryDate = entity.DeliveryDate,
            Type = entity.Type,
            Status = entity.Status
        };
    }

    /// <summary>
    /// Кастомный валидатор для проверки даты в будущем
    /// </summary>
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is null) return true;
            if (value is DateTime date)
                return date > DateTime.UtcNow;

            return false;
        }
    }
}
