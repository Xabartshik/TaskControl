using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;
//TODO: Пересмотреть сущность, добавить в нее некоторые поля, по типу имени пользователя, типа задачи и все такое
namespace TaskControl.TaskModule.Application.DTOs
{
    public record TaskAssignationDto
    {
        public int Id { get; init; }

        [Required]
        public int TaskId { get; init; }

        [Required]
        public int UserId { get; init; }
        public DateTime? StartedAt { get; init; }
        public DateTime? CompletedAt { get; init; }
        public DateTime AssignedAt { get; init; }

        public static TaskAssignation FromDto(TaskAssignationDto dto) => new()
        {
            Id = dto.Id,
            TaskId = dto.TaskId,
            UserId = dto.UserId,
            AssignedAt = dto.AssignedAt
        };

        public static TaskAssignationDto ToDto(TaskAssignation entity) => new()
        {
            Id = entity.Id,
            TaskId = entity.TaskId,
            UserId = entity.UserId,
            AssignedAt = entity.AssignedAt
        };
    }
}
