using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Application.DTOs;
using TaskControl.ReportsModule.Application.Interface;

namespace TaskControl.ReportsModule.Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsQueryProvider _provider;

        public AnalyticsController(IAnalyticsQueryProvider provider)
        {
            _provider = provider;
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
    }
}