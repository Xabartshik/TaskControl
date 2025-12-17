using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.DTOs.InventorizationDTOs
{
    /// <summary>
    /// DTO для аналитики по расхождениям
    /// </summary>
    public class DiscrepancyAnalyticsDto
    {
        public int TotalDiscrepancies { get; set; }
        public int SurplusCount { get; set; }
        public int ShortageCount { get; set; }
        public decimal AccuracyPercentage { get; set; }
        public int MostCommonDiscrepancyType { get; set; } // 0=Нет, 1=Избыток, 2=Нехватка
        public decimal AverageVarianceQuantity { get; set; }
    }
}
