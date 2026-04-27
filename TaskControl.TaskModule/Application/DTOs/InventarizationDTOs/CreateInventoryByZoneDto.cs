using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    public class CreateInventoryByZoneDto
    {
        [Required]
        public List<string> ZonePrefixes { get; set; } = new List<string>();

        [Range(0, 5)]
        public int PriorityLevel { get; set; } = 1;

        public int WorkerCount { get; set; } = 1;

        public List<int>? WorkerIds { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime? DeadlineDate { get; set; }
    }
}
