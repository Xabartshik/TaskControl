using System.Collections.Generic;

namespace TaskControl.ReportsModule.Application.Helpers
{
    public static class ReportLocalizationHelper
    {
        // Перевод статусов заказов
        private static readonly Dictionary<string, string> OrderStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Created", "Создан" },
            { "Assembly", "На сборке" },
            { "Ready", "Готов к выдаче" },
            { "InTransit", "В пути" },
            { "Completed", "Выполнен" },
            { "Cancelled", "Отменен" },
            { "PartiallyCompleted", "Выполнен частично" },
            { "Rejected", "Отклонен" }
        };

        // Перевод типов доставки
        private static readonly Dictionary<string, string> DeliveryTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Pickup", "Самовывоз" },
            { "Delivery", "Доставка курьером" },
            { "Express", "Экспресс (Прямая выдача)" } // Учитываем вашу специфику экспресс-выдачи
        };

        // Перевод статусов задач
        private static readonly Dictionary<string, string> TaskStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            { "New", "Новая" },
            { "Assigned", "Назначена" },
            { "InProgress", "В работе" },
            { "Completed", "Завершена" },
            { "Cancelled", "Отменена" },
            { "OnHold", "Приостановлена" },
            { "Blocked", "Заблокирована" }
        };

        // Перевод типов задач
        private static readonly Dictionary<string, string> TaskTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "OrderAssembly", "Сборка заказа" },
            { "OrderHandover", "Выдача заказа" },
            { "ReturnToStock", "Возврат товара" }
        };

        public static string TranslateOrderStatus(string status) =>
            OrderStatuses.TryGetValue(status, out var ru) ? ru : status;

        public static string TranslateDeliveryType(string type) =>
            DeliveryTypes.TryGetValue(type, out var ru) ? ru : type;

        public static string TranslateTaskStatus(string status) =>
            TaskStatuses.TryGetValue(status, out var ru) ? ru : status;

        public static string TranslateTaskType(string type) =>
            TaskTypes.TryGetValue(type, out var ru) ? ru : type;

        // Вспомогательный метод для форматирования времени (Lead Time)
        public static string FormatDuration(int totalMinutes)
        {
            if (totalMinutes < 0) return "-";
            if (totalMinutes < 60) return $"{totalMinutes} мин.";

            var hours = totalMinutes / 60;
            var mins = totalMinutes % 60;
            return $"{hours} ч. {mins} мин.";
        }
    }
}