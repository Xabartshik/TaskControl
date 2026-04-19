using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    // Заполняется только если Type == "Инвентаризация"
    public record MobileBaseTaskDto(InventoryTaskDetailsDto? InventarizationDetails) : BaseTaskDto
    {
        public string TaskType { get; set; } = string.Empty;
    }
}
