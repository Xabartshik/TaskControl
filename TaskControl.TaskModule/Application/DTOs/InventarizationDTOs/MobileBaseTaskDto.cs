using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    // Заполняется только если Type == "Инвентаризация" (или твой ключ типа)
    public record MobileBaseTaskDto(InventoryTaskDetailsDto? InventarizationDetails) : BaseTaskDto;
}
