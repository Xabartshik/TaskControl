using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
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

        public QrCheckInController(
            IQRTokenService qrTokenService,
            IService<CheckIOEmployeeDto> checkIoService)
        {
            _qrTokenService = qrTokenService;
            _checkIoService = checkIoService;
        }

        public class ScanQrRequestDto
        {
            public string Payload { get; set; } = string.Empty;
            public int BranchId { get; set; }
            public string CheckType { get; set; } = "in"; // "in" или "out"
        }

        [HttpPost("scan")]
        [Authorize] // <-- 1. ОБЯЗАТЕЛЬНО возвращаем атрибут (закрываем эндпоинт от гостей)
        public async Task<IActionResult> ScanQr([FromBody] ScanQrRequestDto request)
        {
            // 1. Валидация подписи и времени (не просрочен ли QR)
            if (!_qrTokenService.ValidateTokenPayload(request.Payload, out string error))
            {
                return BadRequest(new { Message = error });
            }

            // 2. Достаем ID сотрудника из токена
            // Ищем claim с типом "EmployeeId". Иногда используют стандартный ClaimTypes.NameIdentifier
            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId" || c.Type == "id")?.Value;

            // Если токен валидный, но внутри нет EmployeeId, или он не число
            if (string.IsNullOrWhiteSpace(employeeIdClaim) || !int.TryParse(employeeIdClaim, out int employeeId))
            {
                return Unauthorized(new { Message = "Не удалось определить ID сотрудника из токена авторизации. Убедитесь, что токен содержит корректный Claim." });
            }

            // 3. Сохранение отметки в БД с настоящим ID
            var checkDto = new CheckIOEmployeeDto
            {
                EmployeeId = employeeId,
                BranchId = request.BranchId,
                CheckType = request.CheckType,
                CheckTimeStamp = DateTime.UtcNow
            };

            await _checkIoService.Add(checkDto);

            return Ok(new { Message = $"Отметка '{request.CheckType}' успешно сохранена для сотрудника ID: {employeeId}!" });
        }
    }
}