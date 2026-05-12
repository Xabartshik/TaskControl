using Microsoft.Extensions.Options;
using System;
using TaskControl.Core.AppSettings;

namespace TaskControl.TaskModule.Domain
{
    public interface ITaskComplexityCalculator
    {
        /// <summary>
        /// Вычисляет сложность задачи (Effort Points) в double.
        /// </summary>
        /// <param name="uniqueLinesCount">Количество уникальных позиций (SKU)</param>
        /// <param name="totalQuantity">Общее количество товаров</param>
        /// <param name="totalWeightKg">Суммарный вес всех товаров (кг)</param>
        /// <param name="maxSingleItemWeightKg">Вес самого тяжелого товара (кг)</param>
        /// <param name="totalVolumeM3">Общий объем (м³)</param>
        double Calculate(int uniqueLinesCount, int totalQuantity, double totalWeightKg, double maxSingleItemWeightKg, double totalVolumeM3 = 0);

        /// <summary>
        /// Определяет необходимость помощи напарника.
        /// </summary>
        bool RequiresCooperation(double maxSingleItemWeightKg);

        TaskComplexityResult CalculateForItems(IEnumerable<TaskItemMetrics> items);
    }

    public class TaskComplexityCalculator : ITaskComplexityCalculator
    {
        private readonly AppSettings _settings;

        public TaskComplexityCalculator(IOptions<AppSettings> options)
        {
            _settings = options.Value; // Используем настройки из DI
        }

        public double Calculate(int uniqueLinesCount, int totalQuantity, double totalWeightKg, double maxSingleItemWeightKg, double totalVolumeM3 = 0)
        {
            double complexity = 1.0;
            double maxWorkerWeight = _settings.MaxWeightPerWorker; // Значение из конфигурации (по умолчанию 50.0)
            double cartCapacity = _settings.CartCapacityM3; // Значение из конфигурации (по умолчанию 0.15)

            // 1. Учитываем логистику (перемещение между уникальными ячейками)
            complexity += uniqueLinesCount * 1.5;

            // 2. Учитываем физический сбор дополнительных единиц
            int extraItems = totalQuantity - uniqueLinesCount;
            if (extraItems > 0)
            {
                complexity += extraItems * 0.05;
            }

            // 3. Кооперация: если хотя бы один товар весит больше нормы
            if (maxSingleItemWeightKg >= maxWorkerWeight)
            {
                complexity += 10.0 + ((maxSingleItemWeightKg - maxWorkerWeight) * 0.5);
            }

            // 4. Многократные ходки из-за суммарного веса
            if (totalWeightKg > maxWorkerWeight && maxSingleItemWeightKg < maxWorkerWeight)
            {
                double extraTrips = Math.Floor(totalWeightKg / maxWorkerWeight);
                complexity += extraTrips * 2.0;
            }

            // 5. Многократные ходки из-за превышения объема тележки
            if (totalVolumeM3 > cartCapacity)
            {
                double extraCarts = Math.Ceiling(totalVolumeM3 / cartCapacity) - 1;
                complexity += extraCarts * 3.0;
            }

            return Math.Round(complexity, 2);
        }

        public bool RequiresCooperation(double maxSingleItemWeightKg)
        {
            // Проверка на основе индивидуального веса товара из настроек
            return maxSingleItemWeightKg >= _settings.MaxWeightPerWorker;
        }

        public TaskComplexityResult CalculateForItems(IEnumerable<TaskItemMetrics> items)
        {
            double totalWeightKg = 0;
            double totalVolumeM3 = 0;
            double maxSingleItemWeightKg = 0;
            int totalItemsQty = 0;
            int uniqueLinesCount = items.Count();

            foreach (var item in items)
            {
                // 1. Конвертация веса (г -> кг)
                double unitWeightKg = item.WeightGrams / 1000.0;
                if (unitWeightKg > maxSingleItemWeightKg) maxSingleItemWeightKg = unitWeightKg;

                // 2. Конвертация объема (мм³ -> м³)
                double unitVolumeM3 = (item.LengthMm * item.WidthMm * item.HeightMm) / 1_000_000_000.0;

                totalWeightKg += unitWeightKg * item.Quantity;
                totalVolumeM3 += unitVolumeM3 * item.Quantity;
                totalItemsQty += item.Quantity;
            }

            // Вызываем старый базовый метод
            double totalComplexity = Calculate(uniqueLinesCount, totalItemsQty, totalWeightKg, maxSingleItemWeightKg, totalVolumeM3);
            bool requiresHelper = RequiresCooperation(maxSingleItemWeightKg);

            // Распределяем Effort Points: Главному 60% баллов, Помощнику 40%
            return new TaskComplexityResult
            {
                TotalComplexity = totalComplexity,
                MainComplexity = requiresHelper ? Math.Round(totalComplexity * 0.6, 2) : totalComplexity,
                HelperComplexity = requiresHelper ? Math.Round(totalComplexity * 0.4, 2) : 0,
                RequiresHelper = requiresHelper
            };
        }

    }
    public class TaskItemMetrics
    {
        public double WeightGrams { get; set; }
        public double LengthMm { get; set; }
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
        public int Quantity { get; set; }
    }

    public class TaskComplexityResult
    {
        public double TotalComplexity { get; set; }
        public double MainComplexity { get; set; }
        public double HelperComplexity { get; set; }
        public bool RequiresHelper { get; set; }
    }
}