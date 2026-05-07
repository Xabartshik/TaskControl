using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ILogger<QrCheckInController> _logger;

        // Инжектим коллекцию всех модулей, которые хотят реагировать на чекины
        private readonly IEnumerable<IEmployeeCheckInObserver> _checkInObservers;

        public QrCheckInController(
            IQRTokenService qrTokenService,
            IService<CheckIOEmployeeDto> checkIoService,
            ILogger<QrCheckInController> logger,
            IEnumerable<IEmployeeCheckInObserver> checkInObservers)
        {
            _qrTokenService = qrTokenService;
            _checkIoService = checkIoService;
            _logger = logger;
            _checkInObservers = checkInObservers;
        }

        public class ScanQrRequestDto
        {
            public string Payload { get; set; } = string.Empty;
            public int BranchId { get; set; }
            public string CheckType { get; set; } = "in"; // "in", "out", "dock"
        }

        [HttpPost("scan")]
        [Authorize(Roles = "Worker,Supervisor,Admin,Courier")]
        public async Task<IActionResult> ScanQr([FromBody] ScanQrRequestDto request)
        {
            _logger.LogInformation("Попытка отметки. Тип: {CheckType}, Payload: {Payload}", request.CheckType, request.Payload);

            if (!_qrTokenService.ValidateTokenPayload(request.Payload, out string error))
                return BadRequest(new { Message = error });

            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId" || c.Type == "id")?.Value;
            if (string.IsNullOrWhiteSpace(employeeIdClaim) || !int.TryParse(employeeIdClaim, out int employeeId))
                return Unauthorized(new { Message = "Не удалось определить ID сотрудника из токена." });

            var checkDto = new CheckIOEmployeeDto
            {
                EmployeeId = employeeId,
                BranchId = request.BranchId,
                CheckType = request.CheckType,
                CheckTimeStamp = DateTime.UtcNow
            };

            await _checkIoService.Add(checkDto);

            // =========================================================
            // Рассылаем событие всем заинтересованным модулям
            // InformationModule понятия не имеет, кто они и что они делают!
            // =========================================================
            foreach (var observer in _checkInObservers)
            {
                try
                {
                    await observer.OnEmployeeCheckedAsync(employeeId, request.BranchId, request.CheckType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при выполнении слушателя чекинов: {ObserverType}", observer.GetType().Name);
                    // Мы логируем, но не прерываем цикл, чтобы отметка всё равно прошла успешно
                }
            }

            return Ok(new { Message = $"Отметка '{request.CheckType}' успешно сохранена!" });
        }

        // Модель запроса без QR-кода
        public class UpdateStatusRequestDto
        {
            public int BranchId { get; set; }
            public string CheckType { get; set; } = "dispatch"; // "dispatch" или "dock"
        }

        /// <summary>
        /// Эндпоинт для кнопок в мобильном приложении (без сканирования физического QR)
        /// </summary>
        [HttpPost("status")]
        [Authorize(Roles = "Courier")] // Только для курьеров
        public async Task<IActionResult> UpdateMobileStatus([FromBody] UpdateStatusRequestDto request)
        {
            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId" || c.Type == "id")?.Value;
            if (string.IsNullOrWhiteSpace(employeeIdClaim) || !int.TryParse(employeeIdClaim, out int employeeId))
                return Unauthorized(new { Message = "Не удалось определить ID сотрудника из токена." });

            var checkDto = new CheckIOEmployeeDto
            {
                EmployeeId = employeeId,
                BranchId = request.BranchId,
                CheckType = request.CheckType,
                CheckTimeStamp = DateTime.UtcNow
            };

            // Сохраняем отметку
            await _checkIoService.Add(checkDto);

            // Если курьер нажал "Вернулся на базу" - рассылаем событие, чтобы сгенерировать возвраты (ReturnToStock)
            if (request.CheckType == "dock")
            {
                foreach (var observer in _checkInObservers)
                {
                    try { await observer.OnEmployeeCheckedAsync(employeeId, request.BranchId, request.CheckType); }
                    catch (Exception ex) { _logger.LogError(ex, "Ошибка слушателя чекинов"); }
                }
            }

            return Ok(new { Message = $"Статус транспорта '{request.CheckType}' успешно обновлен!" });
        }
    }


}