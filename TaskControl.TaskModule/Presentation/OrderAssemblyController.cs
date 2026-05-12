using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.Presentation
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderAssemblyController : ControllerBase
    {
        private readonly IOrderAssemblyExecutionService _executionService;
        private readonly ILogger<OrderAssemblyController> _logger;

        public OrderAssemblyController(
            IOrderAssemblyExecutionService executionService,
            ILogger<OrderAssemblyController> logger)
        {
            _executionService = executionService;
            _logger = logger;
        }

        private void AddDeprecationWarning(string replacement)
        {
            Response.Headers.Append("Warning", $"299 - Deprecated API. Use {replacement}");
            Response.Headers.Append("X-Api-Deprecated", "true");
        }

        public class ExpressHandoverRequest
        {
            public string QrToken { get; set; }
            // ДОБАВЛЕНО: Словарь отмененных позиций (LineId -> Количество отмены)
            public Dictionary<int, int>? CancelledLines { get; set; }
        }

        public class VerifyQrRequest
        {
            public string QrToken { get; set; }
        }

        [HttpPost("assignment/{assignmentId}/verify-qr")]
        public async Task<IActionResult> VerifyQr(int assignmentId, [FromBody] VerifyQrRequest req)
        {
            try
            {
                var result = await _executionService.VerifyHandoverTokenAsync(assignmentId, req.QrToken);

                if (result.Success)
                    return Ok(new { Message = result.Message });

                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при верификации QR-кода.");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Эндпоинт для выдачи экспресс-заказа "из рук в руки" через сканирование QR клиента
        /// </summary>
        [HttpPost("assignment/{assignmentId}/express-handover")]
        public async Task<IActionResult> HandoverExpressOrder(int assignmentId, [FromBody] ExpressHandoverRequest req)
        {
            try
            {
                // ИЗМЕНЕНИЕ: Передаем req.CancelledLines в сервис
                var result = await _executionService.HandoverExpressOrder(assignmentId, req.QrToken, req.CancelledLines);

                if (result.Success)
                    return Ok(new { Message = result.Message });

                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при экспресс-выдаче.");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>DEPRECATED: используйте GET /api/v1/WorkerTasks/{workerId}/pending.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{workerId}/pending")]
        [HttpGet("worker/{userId}/assignments")]
        public async Task<IActionResult> GetHeaders(int userId)
        {
            AddDeprecationWarning("GET /api/v1/WorkerTasks/{workerId}/pending");
            return Ok(await _executionService.GetAssignmentsHeaderForWorkerAsync(userId));
        }

        /// <summary>DEPRECATED: используйте GET /api/v1/WorkerTasks/{taskId}/details.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/details")]
        [HttpGet("assignment/{id}/details")]
        public async Task<IActionResult> GetDetails(int id)
        {
            AddDeprecationWarning("GET /api/v1/WorkerTasks/{taskId}/details");
            return Ok(await _executionService.GetAssemblyTaskDetailsAsync(id));
        }

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/start?workerId=...</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/start")]
        [HttpPost("assignment/{id}/start")]
        public async Task<IActionResult> Start(int id)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/start?workerId=...");
            return Ok(await _executionService.StartAssemblyAsync(id));
        }

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/pause.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/pause")]
        [HttpPost("assignment/{id}/pause")]
        public async Task<IActionResult> Pause(int id)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/pause");
            return Ok(await _executionService.PauseAssemblyAsync(id));
        }

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/cancel.</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/cancel")]
        [HttpPost("assignment/{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/cancel");
            return Ok(await _executionService.CancelAssemblyAsync(id));
        }

        /// <summary>DEPRECATED: используйте POST /api/v1/WorkerTasks/{taskId}/complete?workerId=...</summary>
        [Obsolete("Legacy endpoint. Use /api/v1/WorkerTasks/{taskId}/complete")]
        [HttpPost("complete/{assignmentId}")]
        public async Task<IActionResult> Complete(int assignmentId)
        {
            AddDeprecationWarning("POST /api/v1/WorkerTasks/{taskId}/complete?workerId=...");
            await _executionService.CompleteAssemblyTask(assignmentId);
            return Ok();
        }

        public class ScanPickRequest { public int LineId { get; set; } public string Barcode { get; set; } }
        public class ScanAssemblyPlaceRequest { public int LineId { get; set; } public string CellCode { get; set; } }

        // 1. Сборка товара (Pick)
        [HttpPost("assignment/{assignmentId}/scan-pick")]
        public async Task<IActionResult> ScanAndPick(int assignmentId, [FromBody] ScanPickRequest req)
        {
            // Здесь мы оставляем старую логику сервиса, так как она работает с товарами
            await _executionService.ScanAndPickItem(req.LineId, req.Barcode);
            return Ok(new { Message = "Товар успешно собран." });
        }

        [HttpPost("assignment/{assignmentId}/scan-place")]
        public async Task<IActionResult> ScanAndPlace(int assignmentId, [FromBody] ScanAssemblyPlaceRequest req, [FromServices] ITaskDataConnection db)
        {
            using var transaction = await ((DataConnection)db).BeginTransactionAsync();
            try
            {
                var assignment = await db.GetTable<OrderAssemblyAssignmentModel>().FirstOrDefaultAsync(a => a.Id == assignmentId);
                if (assignment == null) return NotFound("Назначение не найдено.");

                // Ищем ячейку по текстовому коду ТОЛЬКО внутри текущего филиала
                var branchPositions = await db.GetTable<PositionModel>().Where(p => p.BranchId == assignment.BranchId).ToListAsync();
                var matchedPos = branchPositions.FirstOrDefault(p => GetFullPositionCode(p) == req.CellCode.Trim());

                if (matchedPos == null) return BadRequest($"Ячейка '{req.CellCode}' не найдена на этом складе.");

                var line = await db.GetTable<OrderAssemblyLineModel>()
                    .FirstOrDefaultAsync(l => l.Id == req.LineId && l.OrderAssemblyAssignmentId == assignmentId);

                if (line == null) return NotFound("Строка сборки не найдена.");

                // Обновляем линию: прописываем найденный ID ячейки и ставим статус 2 (Placed)
                await db.GetTable<OrderAssemblyLineModel>()
                    .Where(l => l.Id == req.LineId)
                    .Set(l => l.TargetPositionId, matchedPos.PositionId)
                    .Set(l => l.Status, 2)
                    .UpdateAsync();

                await transaction.CommitAsync();
                return Ok(new { Message = $"Товар привязан к ячейке {req.CellCode}." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при сканировании ячейки для сборки.");
                return StatusCode(500, ex.Message);
            }
        }

        // Вспомогательный метод для сборки строки ячейки из БД
        private string GetFullPositionCode(PositionModel pos)
        {
            if (pos == null) return null;
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(pos.ZoneCode)) parts.Add(pos.ZoneCode);
            if (!string.IsNullOrEmpty(pos.FirstLevelStorageType)) parts.Add(pos.FirstLevelStorageType);
            if (!string.IsNullOrEmpty(pos.FLSNumber)) parts.Add(pos.FLSNumber);
            if (!string.IsNullOrEmpty(pos.SecondLevelStorage)) parts.Add(pos.SecondLevelStorage);
            if (!string.IsNullOrEmpty(pos.ThirdLevelStorage)) parts.Add(pos.ThirdLevelStorage);
            return string.Join("-", parts);
        }

        [HttpPost("scan-place-bulk")]
        public async Task<IActionResult> ScanAndPlaceBulk([FromBody] ScanPlaceBulkRequest req)
        {
            var result = await _executionService.ScanAndPlaceBulk(req.AssignmentId, req.CellCode);
            return Ok(result);
        }

        [HttpPost("report-missing")]
        public async Task<IActionResult> ReportMissing([FromBody] ReportMissingRequest req)
        {
            await _executionService.ReportMissingItem(req.LineId, req.Reason);
            return Ok();
        }
    }

    public class ScanPickRequest { public int LineId { get; set; } public string Barcode { get; set; } }
    public class ScanPlaceBulkRequest { public int AssignmentId { get; set; } public string CellCode { get; set; } }
    public class ReportMissingRequest { public int LineId { get; set; } public string Reason { get; set; } }
}
