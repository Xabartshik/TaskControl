using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // <-- Не забудь
using System.Linq;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.Application.Services;

namespace TaskControl.InformationModule.Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrCheckInController : ControllerBase
    {
        private readonly IQRTokenService _qrTokenService;
        private readonly IService<CheckIOEmployeeDto> _checkIoService;
        private readonly ILogger<QrCheckInController> _logger; // <-- ДОБАВЛЕНО

        public QrCheckInController(
            IQRTokenService qrTokenService,
            IService<CheckIOEmployeeDto> checkIoService,
            ILogger<QrCheckInController> logger) // <-- ДОБАВЛЕНО
        {
            _qrTokenService = qrTokenService;
            _checkIoService = checkIoService;
            _logger = logger;
        }

        public class ScanQrRequestDto
        {
            public string Payload { get; set; } = string.Empty;
            public int BranchId { get; set; }
            public string CheckType { get; set; } = "in";
        }

        [HttpPost("scan")]
        [Authorize(Roles = "Worker,Supervisor,Admin,Courier")] // <-- Проверь, чтобы тут был Courier!
        public async Task<IActionResult> ScanQr([FromBody] ScanQrRequestDto request)
        {
            _logger.LogInformation("Попытка отметки на проходной. Payload: {Payload}", request.Payload);

            // 1. Валидация подписи и времени
            if (!_qrTokenService.ValidateTokenPayload(request.Payload, out string error))
            {
                _logger.LogWarning("Отказ в отметке: {Error}", error);
                return BadRequest(new { Message = error });
            }

            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId" || c.Type == "id")?.Value;

            if (string.IsNullOrWhiteSpace(employeeIdClaim) || !int.TryParse(employeeIdClaim, out int employeeId))
            {
                _logger.LogWarning("Отказ в отметке: Не найден EmployeeId в токене авторизации");
                return Unauthorized(new { Message = "Не удалось определить ID сотрудника из токена авторизации." });
            }

            var checkDto = new CheckIOEmployeeDto
            {
                EmployeeId = employeeId,
                BranchId = request.BranchId,
                CheckType = request.CheckType,
                CheckTimeStamp = DateTime.UtcNow
            };

            await _checkIoService.Add(checkDto);

            _logger.LogInformation("Успешная отметка {Type} для сотрудника {Id}", request.CheckType, employeeId);
            return Ok(new { Message = $"Отметка успешно сохранена!" });
        }
    }
}