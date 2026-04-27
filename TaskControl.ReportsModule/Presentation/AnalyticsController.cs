using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Application.DTOs;
using TaskControl.ReportsModule.Application.Interface;
using TaskControl.ReportsModule.Application.Services;

namespace TaskControl.ReportsModule.Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsQueryProvider _provider;
        private readonly ReportExportService _exportService;

        public AnalyticsController(IAnalyticsQueryProvider provider, ReportExportService exportService)
        {
            _provider = provider;
            _exportService = exportService;
        }

        [HttpPost("worker-efficiency")]
        public async Task<ActionResult<IEnumerable<WorkerEfficiencyResultDto>>> GetWorkerEfficiency([FromBody] AnalyticsQueryDto query)
        {
            var result = await _provider.GetWorkerEfficiencyAsync(query);
            return Ok(result);
        }

        [HttpPost("branch-summary")]
        public async Task<ActionResult<IEnumerable<BranchSummaryResultDto>>> GetBranchSummary([FromBody] AnalyticsQueryDto query)
        {
            // Отдает сводку по филиалам, агрегируя данные на лету
            var result = await _provider.GetBranchSummaryAsync(query);
            return Ok(result);
        }



        [HttpGet("branch-detailed/{branchId}")]
        public async Task<IActionResult> GetBranchDetailedReport(int branchId, [FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] string format = "json")
        {
            if (format.ToLower() == "pdf")
            {
                // Для PDF получаем сгруппированные данные
                var groupedData = await _provider.GetGroupedBranchReportAsync(branchId, start, end);
                var pdfBytes = _exportService.ExportGroupedReportToPdf(groupedData, $"Сводный отчет по филиалу {branchId}");
                return File(pdfBytes, "application/pdf", $"branch_{branchId}_grouped.pdf");
            }

            // Для JSON и CSV оставляем плоский список (он удобнее для машинной обработки)
            var flatData = await _provider.GetDetailedBranchReportAsync(branchId, start, end);

            if (format.ToLower() == "csv")
                return File(_exportService.ExportToCsv(flatData), "text/csv", $"branch_{branchId}_report.csv");

            return Ok(flatData);
        }

        [HttpGet("worker-detailed/{workerId}")]
        public async Task<IActionResult> GetWorkerDetailedReport(int workerId, [FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] string format = "json")
        {
            var data = await _provider.GetDetailedWorkerReportAsync(workerId, start, end);

            if (format.ToLower() == "csv")
                return File(_exportService.ExportToCsv(data), "text/csv", $"worker_{workerId}_report.csv");

            return Ok(data);
        }
    }
}