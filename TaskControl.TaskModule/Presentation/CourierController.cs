using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using TaskControl.InformationModule.Application.Services;

namespace TaskControl.TaskModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CourierController : ControllerBase
    {
        private readonly IQRTokenService _qrTokenService;

        public CourierController(IQRTokenService qrTokenService)
        {
            _qrTokenService = qrTokenService;
        }

        /// <summary>
        /// Получить временный QR-код курьера для подтверждения приемки товаров от кладовщика.
        /// Код действителен 5 минут.
        /// </summary>
        [HttpGet("{courierId}/pickup-qr")]
        // [Authorize(Roles = "Courier")] // Раскомментируй для защиты эндпоинта
        public IActionResult GetPickupQrToken(int courierId)
        {
            if (courierId <= 0)
            {
                return BadRequest(new { Message = "Некорректный ID курьера." });
            }

            try
            {
                // Генерируем 5-минутный токен для "рукопожатия"
                string token = _qrTokenService.GenerateCourierPickupToken(courierId);

                return Ok(new
                {
                    Token = token,
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
                    ExpiresInSeconds = 300
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при генерации QR-кода курьера.", Details = ex.Message });
            }
        }
    }
}