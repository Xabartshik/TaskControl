using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.Core.AppSettings
{
    public class AppSettings
    {
        public const string SectionName = "AppSettings";
        public bool EnableDetailedLogging { get; set; } = false;
        public string QrHmacSecretKey { get; set; } = string.Empty;
        public double MaxWeightPerWorker { get; set; } = 50.0;
        public int PriorityChangeTimeMinutes { get; set; } = 30;
        public double MaxRestingWorkersPercentage { get; set; } = 0.2; // 20%
        public double PickupWindowLimitHours { get; set; } = 0.5; // 30 минут = 0.5 часа
        public double DeliveryWindowLimitHours { get; set; } = 2.0;
        public double WeightCoefficient { get; set; } = 0.5;
        public int WorkMinutesRequiredForBreak { get; set; } = 50; // Число минут непрерывного труда для накопления перерыва
        public int BreakDurationMinutes { get; set; } = 10;        // Длительность перерыва по ТК РФ
        public int MaxConcurrentBreaksPercentage { get; set; } = 20; // Лимит отдыхающих (20% от смены)
    }
}
