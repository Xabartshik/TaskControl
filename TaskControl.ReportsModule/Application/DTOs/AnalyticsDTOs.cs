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