using System;
using System.Collections.Generic;

namespace TaskControl.TaskModule.Application.DTOs.BossPanelDTOs
{
    public class BossPanelTaskCardDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TaskType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public int OverallProgressPercentage { get; set; }
        public List<TaskAssigneeProgressDto> Assignees { get; set; } = new();
    }

    public class TaskAssigneeProgressDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public int AssignedVolume { get; set; }
        public int CompletedVolume { get; set; }
        public string Status { get; set; } // Ожидается, В процессе, Завершено
    }

    public class EmployeeWorkloadDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public bool IsAtWork { get; set; }
        public int ActiveTasksCount { get; set; }
        public List<ActiveTaskBriefDto> ActiveTasks { get; set; } = new();
    }

    public class ActiveTaskBriefDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string TaskType { get; set; }
        public string Status { get; set; }
    }

    public class AvailableEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public bool IsAtWork { get; set; }
        public int ActiveTasksCount { get; set; }
        public bool IsRecommended { get; set; }
    }
    public class AvailableOrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public string DeliveryType { get; set; }
        public string PaymentType { get; set; }

        // Логистика: Адрес клиента (для Delivery) или Постамата
        public string? DestinationAddress { get; set; }

        // Детали Постамата (для демонстрации OMS)
        public string? PostamatAddress { get; set; }
        public string? PostamatCellNumber { get; set; }
        public string? PostamatCellSize { get; set; }

        // Состав заказа (Важно для комиссии)
        public List<OrderItemDetailDto> Items { get; set; } = new();

        public bool IsHighPriority => DeliveryType == "Express";
    }

    public class OrderItemDetailDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
