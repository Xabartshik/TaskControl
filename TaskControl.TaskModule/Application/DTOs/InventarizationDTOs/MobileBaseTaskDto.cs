using System;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    public record MobileBaseTaskDto : BaseTaskDto
    {
        public string TaskType { get; set; } = string.Empty;
        public AssignmentStatus AssignmentStatus { get; set; } = AssignmentStatus.Assigned;
        public object? TaskDetails { get; init; }
    }
}