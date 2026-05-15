using System;
using TaskControl.ReportsModule.Application.Helpers;

namespace TaskControl.ReportsModule.Application.DTOs
{
    // Блок 1: Аналитика заказов (SLA)
    public class OrderLeadTimeDto
    {
        public int OrderId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTypeRaw { get; set; } = string.Empty;
        public string StatusRaw { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }

        // Локализованные свойства для отчета
        public string DeliveryType => ReportLocalizationHelper.TranslateDeliveryType(DeliveryTypeRaw);
        public string Status => ReportLocalizationHelper.TranslateOrderStatus(StatusRaw);

        // Время жизни заказа в минутах
        public int LeadTimeMinutes => DeliveryDate.HasValue
            ? (int)(DeliveryDate.Value - CreatedAt).TotalMinutes
            : (int)(DateTime.UtcNow - CreatedAt).TotalMinutes;

        public string LeadTimeFormatted => ReportLocalizationHelper.FormatDuration(LeadTimeMinutes);
    }

    // Блок 2: Товарная аналитика
    public class TopItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty; // Название вместо ID!
        public int QuantitySold { get; set; }

        // В БД хранятся граммы и миллиметры
        public double TotalWeightGrams { get; set; }
        public double TotalVolumeCubicMm { get; set; }

        // Красивые свойства для отчета (кг и м³)
        public double TotalWeightKg => Math.Round(TotalWeightGrams / 1000.0, 2);

        // 1 кубический метр = 1 000 000 000 куб. мм
        public double TotalVolumeCubicMeters => Math.Round(TotalVolumeCubicMm / 1_000_000_000.0, 4);
    }



    // Блок 3: KPI сотрудников (Эффективность)
    public class EmployeeKpiDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty; // Фамилия + Имя
        public string BranchName { get; set; } = string.Empty;

        public int TasksCompleted { get; set; }
        public int TotalDurationSeconds { get; set; }

        public double TotalWeightMovedGrams { get; set; }
        public double TotalVolumeMovedCubicMm { get; set; }

        // Форматированные данные для вывода
        public double TotalWeightMovedKg => Math.Round(TotalWeightMovedGrams / 1000.0, 2);
        public double TotalVolumeMovedCubicMeters => Math.Round(TotalVolumeMovedCubicMm / 1_000_000_000.0, 4);
        public string AverageTaskDuration => TasksCompleted > 0
            ? ReportLocalizationHelper.FormatDuration(TotalDurationSeconds / TasksCompleted / 60)
            : "0 мин.";
        public int TotalComplexity { get; set; }
    }

    public class EmployeeTaskDetailDto
    {
        public int TaskId { get; set; }
        public string TaskTypeRaw { get; set; } = string.Empty;
        public string TaskType => ReportLocalizationHelper.TranslateTaskType(TaskTypeRaw);
        public DateTime? CompletedAt { get; set; }
        public int Complexity { get; set; }
        public int DurationSeconds { get; set; }
        public string DurationFormatted => ReportLocalizationHelper.FormatDuration(DurationSeconds / 60);
    }
    public class EmployeeTaskFlatDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;

        public int TaskId { get; set; }
        public string TaskTypeRaw { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int Complexity { get; set; }
        public int ReactionTimeSeconds { get; set; } // Время от Assigned до Started
        public int ExecutionTimeSeconds { get; set; } // Время от Started до Completed

        public double WeightGrams { get; set; }
        public double VolumeCubicMm { get; set; }
    }

    // Иерархическая карточка сотрудника для PDF
    public class EmployeeFullReportDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;

        // Сводные KPI
        public int TotalTasksCompleted => Tasks.Count;
        public int TotalComplexity => Tasks.Sum(t => t.Complexity);
        public double TotalWeightKg => Math.Round(Tasks.Sum(t => t.WeightGrams) / 1000.0, 2);
        public double TotalVolumeCubicMeters => Math.Round(Tasks.Sum(t => t.VolumeCubicMm) / 1_000_000_000.0, 4);

        public string AverageReactionTime => TotalTasksCompleted > 0
            ? ReportLocalizationHelper.FormatDuration((int)Tasks.Average(t => t.ReactionTimeSeconds) / 60) : "0 мин.";

        public string AverageExecutionTime => TotalTasksCompleted > 0
            ? ReportLocalizationHelper.FormatDuration((int)Tasks.Average(t => t.ExecutionTimeSeconds) / 60) : "0 мин.";

        // Список всех задач
        public List<EmployeeTaskExtendedDto> Tasks { get; set; } = new();
    }

    // Вложенная задача внутри карточки сотрудника
    public class EmployeeTaskExtendedDto
    {
        public int TaskId { get; set; }
        public string TaskTypeRaw { get; set; } = string.Empty;
        public string TaskType => ReportLocalizationHelper.TranslateTaskType(TaskTypeRaw);

        public DateTime CreatedAt { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int ReactionTimeSeconds { get; set; }
        public int ExecutionTimeSeconds { get; set; }
        public string ReactionTimeFormatted => ReportLocalizationHelper.FormatDuration(ReactionTimeSeconds / 60);
        public string ExecutionTimeFormatted => ReportLocalizationHelper.FormatDuration(ExecutionTimeSeconds / 60);

        public int Complexity { get; set; }
        public double WeightGrams { get; set; }
        public double VolumeCubicMm { get; set; }
        public double WeightKg => Math.Round(WeightGrams / 1000.0, 2);
    }

    // 
    // БЛОК 2: УПРАВЛЕНЧЕСКИЙ ДАШБОРД ЗАКАЗОВ
    // 

    public class OrderRegistryFlatDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string DeliveryTypeRaw { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string StatusRaw { get; set; } = string.Empty;

        // Новые поля для дат
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualHandoverAt { get; set; }

        public string ItemsList { get; set; } = string.Empty;
        public double TotalWeightGrams { get; set; }
        public double TotalVolumeCubicMm { get; set; }

        public string DeliveryType => ReportLocalizationHelper.TranslateDeliveryType(DeliveryTypeRaw);
        public string Status => ReportLocalizationHelper.TranslateOrderStatus(StatusRaw);
        public double TotalWeightKg => Math.Round(TotalWeightGrams / 1000.0, 2);
        public double TotalVolumeCubicMeters => Math.Round(TotalVolumeCubicMm / 1_000_000_000.0, 4);

        // Lead Time теперь считается по фактическому времени выдачи, а не по ожидаемому
        public int LeadTimeMinutes => ActualHandoverAt.HasValue
            ? (int)(ActualHandoverAt.Value - CreatedAt).TotalMinutes
            : (int)(DateTime.UtcNow - CreatedAt).TotalMinutes;
        public string LeadTimeFormatted => ReportLocalizationHelper.FormatDuration(LeadTimeMinutes);
    }


    public class OrderDashboardReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageCheck => TotalOrders > 0 ? Math.Round(TotalRevenue / TotalOrders, 2) : 0;

        // Рекорды (для блока сверху)
        public OrderRegistryFlatDto? HeaviestOrder { get; set; }
        public OrderRegistryFlatDto? MostVoluminousOrder { get; set; }
        public OrderRegistryFlatDto? FastestOrder { get; set; }

        // Топ товары (встроим их прямо в дашборд)
        public List<TopItemDto> TopItems { get; set; } = new();

        // Полный список для таблицы
        public List<OrderRegistryFlatDto> Orders { get; set; } = new();
    }

    // НОВЫЙ КЛАСС: Детализация заказа с товарами и габаритами
    public class OrderDetailedDto
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StatusRaw { get; set; } = string.Empty;
        public string Status => ReportLocalizationHelper.TranslateOrderStatus(StatusRaw);

        // Сгруппированный список товаров в заказе: "Ноутбук (2 шт), Мышка (1 шт)"
        public string ItemsList { get; set; } = string.Empty;

        public double TotalWeightGrams { get; set; }
        public double TotalVolumeCubicMm { get; set; }

        public double TotalWeightKg => Math.Round(TotalWeightGrams / 1000.0, 2);
        public double TotalVolumeCubicMeters => Math.Round(TotalVolumeCubicMm / 1_000_000_000.0, 4);
    }

    // Блок 4: Сводка по филиалам
    public class BranchSummaryDto
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int TotalOrdersCompleted { get; set; }
        public decimal TotalRevenue { get; set; }
        public int AverageLeadTimeMinutes { get; set; }

        public string AverageLeadTimeFormatted => ReportLocalizationHelper.FormatDuration(AverageLeadTimeMinutes);
    }

    // Общий фильтр для запроса отчетов
    public class AnalyticsFilterDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? BranchId { get; set; } // Null означает все филиалы
        public int? EmployeeId { get; set; } // Null означает всех сотрудников
    }
}