using System;
using TaskControl.TaskModule.Application.DTOs;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    public record MobileBaseTaskDto : BaseTaskDto
    {
        public string TaskType { get; set; } = string.Empty;
        public object? TaskDetails { get; init; }
    }
}