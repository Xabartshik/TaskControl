using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using TaskControl.TaskModule.Domain;
namespace TaskControl.TaskModule.Application.DTOs
{
    public record BaseTaskDto
    {
        public int TaskId { get; init; }

        [Required]
        public int BranchId { get; init; }

        [Required]
        [StringLength(50)]
        public string Type { get; init; }

        public DateTime CreatedAt { get; init; }
        public DateTime? CompletedAt { get; init; }

        [Required]
        [RegularExpression("^(New|InProgress|Completed|Cancelled)$")]
        public string Status { get; init; }

        public JsonElement? JSONParams { get; init; }

        public static BaseTask FromDto(BaseTaskDto dto) => new()
        {
            TaskId = dto.TaskId,
            BranchId = dto.BranchId,
            Type = dto.Type,
            CreatedAt = dto.CreatedAt,
            CompletedAt = dto.CompletedAt,
            Status = dto.Status
        };

        public static BaseTaskDto ToDto(BaseTask entity) => new()
        {
            TaskId = entity.TaskId,
            BranchId = entity.BranchId,
            Type = entity.Type,
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.CompletedAt,
            Status = entity.Status
        };
    }
}
