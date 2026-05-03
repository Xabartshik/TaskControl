using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.Services.BackgroundServices;
using TaskControl.TaskModule.Application.Providers;
using TaskControl.TaskModule.Application.Services;

namespace TaskControl.TaskModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")] // Используем версионирование, как у тебя в WorkerTasksController
    public class OrderHandoverController : ControllerBase
    {
        private readonly OrderHandoverExecutionProvider _provider;
        private readonly HandoverTaskGeneratorService _generatorService;
        private readonly IQRTokenService _qrTokenService;

        public OrderHandoverController(OrderHandoverExecutionProvider provider, HandoverTaskGeneratorService generatorService, IQRTokenService qrTokenService)
        {
            _provider = provider;
            _generatorService = generatorService;
            _qrTokenService = qrTokenService;
        }

        // Этот метод уникален только для процесса выдачи!
        [HttpPost("{taskId}/scan")]
        public async Task<IActionResult> Scan(int taskId, [FromQuery] int workerId, [FromBody] string barcode)
        {
            var (success, message) = await _provider.ProcessScanAsync(taskId, workerId, barcode);
            if (!success) return BadRequest(new { Error = message });

            return Ok(new { Message = message });
        }

        public record InitHandoverRequest(string QrToken, int WorkerId, int BranchId);

        [HttpPost("init-customer")]
        public async Task<IActionResult> InitCustomerHandover([FromBody] InitHandoverRequest request)
        {
            // 1. Расшифровываем QR код клиента
            if (!_qrTokenService.ValidateOrderPickupToken(request.QrToken, out int customerId, out int orderId, out string error))
            {
                return BadRequest(new { Message = $"Неверный QR-код: {error}" });
            }

            // 2. Генерируем задачу
            try
            {
                int taskId = await _generatorService.CreateHandoverToCustomerTaskAsync(orderId, request.WorkerId, request.BranchId);

                // 3. Возвращаем TaskId обратно в мобилку
                return Ok(new { TaskId = taskId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при создании задачи выдачи", Details = ex.Message });
            }
        }
    }
}