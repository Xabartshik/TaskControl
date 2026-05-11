using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Application.DTOs;
using TaskControl.ReportsModule.Application.Interface;
using TaskControl.ReportsModule.DataAccess.Interface;

namespace TaskControl.ReportsModule.Application.Services
{
    public class OrderAnalyticsService : IOrderAnalyticsService
    {
        private readonly IOrderAnalyticsRepository _repository;
        private readonly ReportExportService _exportService; // Раскомментируем, когда перепишем генератор
                                                             // Добавьте в TaskControl.ReportsModule.Application.Services.OrderAnalyticsService

        public async Task<List<EmployeeFullReportDto>> GetEmployeeFullReportsAsync(AnalyticsFilterDto filter)
        {
            return await _repository.GetEmployeeFullReportsAsync(filter);
        }

        public async Task<OrderDashboardReportDto> GetOrderDashboardAsync(AnalyticsFilterDto filter)
        {
            return await _repository.GetOrderDashboardAsync(filter);
        }

        public async Task<byte[]> GenerateEmployeeFullReportPdfAsync(AnalyticsFilterDto filter)
        {
            var data = await _repository.GetEmployeeFullReportsAsync(filter);

            // Передаем даты из фильтра в генератор для отображения периода
            return _exportService.GenerateEmployeeFullReportPdf(data, "Аналитический отчет по персоналу", filter.StartDate, filter.EndDate);
        }

        public async Task<byte[]> GenerateOrderDashboardPdfAsync(AnalyticsFilterDto filter)
        {
            var data = await _repository.GetOrderDashboardAsync(filter);
            return _exportService.GenerateOrderDashboardPdf(data, "Управленческий дашборд заказов", filter.StartDate, filter.EndDate);
        }
        public OrderAnalyticsService(IOrderAnalyticsRepository repository, ReportExportService exportService)
        {
            _repository = repository;
            _exportService = exportService;
        }
        public async Task<byte[]> GenerateDetailedOrdersPdfAsync(AnalyticsFilterDto filter)
        {
            // 1. Получаем детальные данные из репозитория
            var data = await _repository.GetDetailedOrdersAsync(filter);

            // 2. Генерируем PDF через экспорт-сервис
            return _exportService.GenerateDetailedOrdersPdf(data, "Детальный отчет по управлению заказами");
        }
        public async Task<List<OrderLeadTimeDto>> GetOrderLeadTimesAsync(AnalyticsFilterDto filter)
        {
            return await _repository.GetOrderLeadTimesAsync(filter);
        }

        public async Task<List<TopItemDto>> GetTopItemsAsync(AnalyticsFilterDto filter)
        {
            return await _repository.GetTopItemsAsync(filter);
        }

        public async Task<List<OrderDetailedDto>> GetDetailedOrdersAsync(AnalyticsFilterDto filter)
        {
            // Вызываем метод репозитория, который мы написали в "Шаге 3" (предыдущее сообщение)
            return await _repository.GetDetailedOrdersAsync(filter);
        }

        public async Task<List<EmployeeTaskDetailDto>> GetEmployeeTasksDetailAsync(int employeeId, AnalyticsFilterDto filter)
        {
            return await _repository.GetEmployeeTasksDetailAsync(employeeId, filter);
        }

        // Пример проброса генерации PDF для популярных товаров
        public async Task<byte[]> GenerateTopItemsPdfAsync(AnalyticsFilterDto filter)
        {
            var data = await _repository.GetTopItemsAsync(filter);
            // Вызываем метод генерации из ReportExportService, который мы обновили (с графиками)
            return _exportService.GenerateTopItemsPdf(data, "Аналитика: Популярные товары и объемы");
        }

        public async Task<List<EmployeeKpiDto>> GetEmployeeKpiAsync(AnalyticsFilterDto filter)
        {
            return await _repository.GetEmployeeKpiAsync(filter);
        }

        public async Task<List<BranchSummaryDto>> GetBranchSummaryAsync(AnalyticsFilterDto filter)
        {
            return await _repository.GetBranchSummaryAsync(filter);
        }
    }
}