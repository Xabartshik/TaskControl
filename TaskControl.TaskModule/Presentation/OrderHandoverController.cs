using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.Presentation;
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
        private readonly ILogger<QrCheckInController> _logger;

        public OrderHandoverController(OrderHandoverExecutionProvider provider, HandoverTaskGeneratorService generatorService, IQRTokenService qrTokenService, ILogger<QrCheckInController> logger)
        {
            _provider = provider;
            _logger = logger;
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

        /// <summary>
        /// DTO для передачи QR-кода курьера
        /// </summary>
        public record CompleteCourierRequest(int WorkerId, string QrToken);

        /// <summary>
        /// Подтверждение отгрузки курьеру через сканирование его QR-кода
        /// </summary>
        [HttpPost("{taskId}/complete-courier")]
        public async Task<IActionResult> CompleteCourierHandover(int taskId, [FromBody] CompleteCourierRequest request)
        {
            // Вызываем логику проверки и завершения в провайдере
            var (success, message) = await _provider.TryCompleteWithCourierQrAsync(
                taskId,
                request.WorkerId,
                request.QrToken
            );

            if (!success)
            {
                return BadRequest(new { Error = message });
            }

            return Ok(new { Message = message });
        }

        // Добавляем ExpectedOrderId в запрос
        public record InitHandoverRequest(string QrToken, int WorkerId, int BranchId, int ExpectedOrderId);

        [HttpPost("init-customer")]
        public async Task<IActionResult> InitCustomerHandover([FromBody] InitHandoverRequest request)
        {
            // 1. Расшифровываем QR код клиента
            if (!_qrTokenService.ValidateOrderPickupToken(request.QrToken, out int customerId, out int orderIdFromQr, out string error))
            {
                return BadRequest(new { Message = $"Неверный QR-код: {error}" });
            }

            // --- НОВАЯ ЖЕСТКАЯ ПРОВЕРКА ---
            if (request.ExpectedOrderId != 0 && orderIdFromQr != request.ExpectedOrderId)
            {
                _logger.LogWarning("Курьер {WorkerId} попытался выдать заказ {QrOrderId} вместо {ExpectedOrderId}",
                    request.WorkerId, orderIdFromQr, request.ExpectedOrderId);

                return BadRequest(new { Message = $"Чужой QR-код! Вы сканируете код для заказа #{orderIdFromQr}, а выбрали в списке заказ #{request.ExpectedOrderId}." });
            }
            // -----------------------------

            // 2. Генерируем задачу
            try
            {
                int taskId = await _generatorService.CreateHandoverToCustomerTaskAsync(orderIdFromQr, request.WorkerId, request.BranchId);

                // 3. Возвращаем TaskId обратно в мобилку
                return Ok(new { TaskId = taskId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при создании задачи выдачи", Details = ex.Message });
            }
        }

        /// <summary>
        /// DTO для запроса пакетной выдачи курьеру
        /// </summary>
        public record InitCourierBatchRequest(List<int> OrderIds, int CourierId, int BranchId);

        /// <summary>
        /// Инициализация задачи передачи массива заказов курьеру
        /// </summary>
        [HttpPost("init-courier-batch")]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> InitCourierBatchHandover([FromBody] InitCourierBatchRequest request)
        {
            if (request.OrderIds == null || !request.OrderIds.Any())
                return BadRequest(new { Message = "Список заказов пуст." });

            if (request.CourierId <= 0)
                return BadRequest(new { Message = "Не выбран курьер." });

            try
            {
                // Вызываем метод пакетной генерации, который мы создали ранее в HandoverTaskGeneratorService
                int baseTaskId = await _generatorService.CreateBatchHandoverToCourierTaskAsync(
                    request.OrderIds,
                    request.CourierId,
                    request.BranchId
                );

                return Ok(new
                {
                    Message = "Маршрутный лист сформирован! Задача отгрузки добавлена в очередь склада.",
                    TaskId = baseTaskId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка сервера при формировании маршрута.", Details = ex.Message });
            }
        }
    }
}