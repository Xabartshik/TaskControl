using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Application.DTOs;
using TaskControl.ReportsModule.Application.Interface;
using TaskControl.ReportsModule.Application.Services;

namespace TaskControl.ReportsModule.Presentation
{
    [ApiController]
    [Route("api/analytics")] // Сделаем красивый короткий роут
    public class OrderAnalyticsController : ControllerBase
    {
        private readonly IOrderAnalyticsService _analyticsService;
        private readonly ReportExportService _exportService;

        public OrderAnalyticsController(IOrderAnalyticsService analyticsService, ReportExportService exportService)
        {
            _analyticsService = analyticsService;
            _exportService = exportService;
        }

        // Вспомогательный метод для установки дат по умолчанию, если клиент их не прислал
        private void EnsureValidDates(AnalyticsFilterDto filter)
        {
            if (filter.StartDate == default)
                filter.StartDate = DateTime.UtcNow.AddDays(-30); // За последние 30 дней по умолчанию

            if (filter.EndDate == default)
                filter.EndDate = DateTime.UtcNow;
        }

        [HttpGet("orders/lead-time")]
        public async Task<IActionResult> GetLeadTimes([FromQuery] AnalyticsFilterDto filter)
        {
            EnsureValidDates(filter);
            var result = await _analyticsService.GetOrderLeadTimesAsync(filter);
            return Ok(result);
        }

        [HttpGet("orders/top-items")]
        public async Task<IActionResult> GetTopItems([FromQuery] AnalyticsFilterDto filter)
        {
            EnsureValidDates(filter);
            var result = await _analyticsService.GetTopItemsAsync(filter);
            return Ok(result);
        }

        [HttpGet("export/employees/pdf")]
        public async Task<IActionResult> ExportEmployeeKpiPdf([FromQuery] AnalyticsFilterDto filter)
        {
            EnsureValidDates(filter);
            var data = await _analyticsService.GetEmployeeKpiAsync(filter);

            // Получаем байты PDF
            var pdfBytes = _exportService.GenerateEmployeeKpiPdf(data);

            return File(pdfBytes, "application/pdf", $"Employee_KPI_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet("export/orders/pdf")]
        public async Task<IActionResult> ExportOrderLeadTimePdf([FromQuery] AnalyticsFilterDto filter)
        {
            EnsureValidDates(filter);

            // ВАЖНО: Если вы хотите видеть СОСТАВ ЗАКАЗА (товары, вес), 
            // используйте метод GetDetailedOrdersAsync и соответствующий PDF генератор
            var data = await _analyticsService.GetDetailedOrdersAsync(filter);
            var pdfBytes = await _analyticsService.GenerateDetailedOrdersPdfAsync(filter);

            return File(pdfBytes, "application/pdf", $"Detailed_Orders_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet("export/orders/csv")]
        public async Task<IActionResult> ExportOrderLeadTimeCsv([FromQuery] AnalyticsFilterDto filter)
        {
            EnsureValidDates(filter);
            var data = await _analyticsService.GetOrderLeadTimesAsync(filter);

            var csvBytes = _exportService.ExportToCsv(data);

            // Специфичный MIME-тип для правильного открытия в Excel
            return File(csvBytes, "text/csv", $"Order_Analytics_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet("employees/kpi")]
        public async Task<IActionResult> GetEmployeeKpi([FromQuery] AnalyticsFilterDto filter)
        {
            EnsureValidDates(filter);
            var result = await _analyticsService.GetEmployeeKpiAsync(filter);
            return Ok(result);
        }

        [HttpGet("branches/summary")]
        public async Task<IActionResult> GetBranchSummary([FromQuery] AnalyticsFilterDto filter)
        {
            EnsureValidDates(filter);
            var result = await _analyticsService.GetBranchSummaryAsync(filter);
            return Ok(result);
        }
    }
}