using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.InformationModule.Application.Services;
using TaskControl.OrderModule.Application.Interface;

namespace TaskControl.OrderModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryController : ControllerBase
    {
        private readonly IQRTokenService _qrTokenService;
        private readonly IOrderService _orderService;
        private readonly ILogger<DeliveryController> _logger;

        public DeliveryController(
            IQRTokenService qrTokenService,
            IOrderService orderService,
            ILogger<DeliveryController> logger)
        {
            _qrTokenService = qrTokenService;
            _orderService = orderService;
            _logger = logger;
        }

        public class ValidateQrRequest
        {
            public string QrToken { get; set; }
        }

        /// <summary>
        /// Эндпоинт для ТСД/Телефона сотрудника. Проверяет отсканированный QR-код клиента.
        /// </summary>
        [HttpPost("validate-client-qr")]
        public async Task<IActionResult> ValidateClientQr([FromBody] ValidateQrRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.QrToken))
            {
                return BadRequest(new { message = "QR-код не может быть пустым" });
            }

            // 1. Расшифровываем и валидируем подпись + время
            if (!_qrTokenService.ValidateOrderPickupToken(request.QrToken, out int customerId, out int orderId, out string errorMessage))
            {
                _logger.LogWarning("Попытка использования невалидного QR-кода. Ошибка: {Error}", errorMessage);
                return BadRequest(new { message = errorMessage });
            }

            // 2. Достаем заказ, чтобы убедиться, что он существует и принадлежит клиенту
            var order = await _orderService.GetById(orderId);
            if (order == null || order.CustomerId != customerId)
            {
                return BadRequest(new { message = "Заказ не найден или не принадлежит данному клиенту." });
            }

            // На этом этапе QR валиден. 
            // Возвращаем данные заказа, чтобы приложение сотрудника перешло к этапу выдачи.
            return Ok(new
            {
                message = "QR-код подтвержден",
                orderId = order.OrderId,
                customerId = order.CustomerId,
                deliveryType = order.DeliveryType.ToString()
            });
        }
    }
}