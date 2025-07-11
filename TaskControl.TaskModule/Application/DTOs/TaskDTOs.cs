using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using TaskControl.TaskModule.Domain;
namespace TaskControl.TaskModule.Application.DTOs
{
    public record TaskDto
    {
        public int TaskId { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
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

        public static ActiveTask FromDto(TaskDto dto) => new()
        {
            TaskId = dto.TaskId,
            BranchId = dto.BranchId,
            Type = dto.Type,
            CreatedAt = dto.CreatedAt,
            CompletedAt = dto.CompletedAt,
            Status = dto.Status,
            JSONParams = dto.JSONParams.HasValue ?
                JsonDocument.Parse(dto.JSONParams.Value.ToString()) : null
        };

        public static TaskDto ToDto(ActiveTask entity) => new()
        {
            TaskId = entity.TaskId,
            BranchId = entity.BranchId,
            Type = entity.Type,
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.CompletedAt,
            Status = entity.Status,
            JSONParams = entity.JSONParams?.RootElement
        };
    }
}
