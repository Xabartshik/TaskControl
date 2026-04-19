using System;

namespace TaskControl.TaskModule.Application.DTOs
{
    /// <summary>
    /// Универсальный DTO для уведомления сотрудника о новых задачах
    /// </summary>
    public class WorkloadNotificationDto
    {
        // Идентификатор задачи в системе
        public int TaskId { get; set; }
        
        // Тип задачи (например, "Inventory", "OrderAssembly")
        public string TaskType { get; set; } = string.Empty;
        
        // Описание задачи для работника
        public string Description { get; set; } = string.Empty;
    }
}
