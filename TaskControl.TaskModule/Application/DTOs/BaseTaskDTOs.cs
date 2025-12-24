using System;
using System.ComponentModel.DataAnnotations;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.Application.DTOs
{
    /// <summary>
    /// DTO для передачи данных базовой задачи
    /// </summary>
    public record BaseTaskDto
    {
        public int TaskId { get; init; }

        [Required(ErrorMessage = "Название задачи обязательно")]
        [StringLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        public string Title { get; init; }

        [StringLength(2000, ErrorMessage = "Описание не может превышать 2000 символов")]
        public string? Description { get; init; }

        [Required(ErrorMessage = "Филиал обязателен")]
        public int BranchId { get; init; }

        [Required(ErrorMessage = "Тип задачи обязателен")]
        [StringLength(50, ErrorMessage = "Тип задачи не может превышать 50 символов")]
        public string Type { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? CompletedAt { get; init; }

        [Required]
        public TaskStatus Status { get; init; }

        [Range(0, 10, ErrorMessage = "Приоритет должен быть от 0 до 10")]
        public int Priority { get; init; }

        public static BaseTask FromDto(BaseTaskDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new BaseTask
            {
                TaskId = dto.TaskId,
                Title = dto.Title,
                Description = dto.Description,
                BranchId = dto.BranchId,
                Type = dto.Type,
                CreatedAt = dto.CreatedAt,
                CompletedAt = dto.CompletedAt,
                Status = dto.Status,
                Priority = dto.Priority
            };
        }

        public static BaseTaskDto ToDto(BaseTask entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new BaseTaskDto
            {
                TaskId = entity.TaskId,
                Title = entity.Title,
                Description = entity.Description,
                BranchId = entity.BranchId,
                Type = entity.Type,
                CreatedAt = entity.CreatedAt,
                CompletedAt = entity.CompletedAt,
                Status = entity.Status,
                Priority = entity.Priority
            };
        }
    }

    /// <summary>
    /// DTO для создания новой задачи
    /// </summary>
    public record CreateBaseTaskDto
    {
        [Required(ErrorMessage = "Название задачи обязательно")]
        [StringLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        public string Title { get; init; }

        [StringLength(2000, ErrorMessage = "Описание не может превышать 2000 символов")]
        public string? Description { get; init; }

        [Required(ErrorMessage = "Филиал обязателен")]
        public int BranchId { get; init; }

        [Required(ErrorMessage = "Тип задачи обязателен")]
        [StringLength(50, ErrorMessage = "Тип задачи не может превышать 50 символов")]
        public string Type { get; init; }

        [Range(0, 10, ErrorMessage = "Приоритет должен быть от 0 до 10")]
        public int Priority { get; init; } = 5;
    }

    /// <summary>
    /// DTO для обновления статуса задачи
    /// </summary>
    public record UpdateTaskStatusDto
    {
        [Required]
        public int TaskId { get; init; }

        [Required]
        public TaskStatus Status { get; init; }

        public DateTime? CompletedAt { get; init; }
    }
}
