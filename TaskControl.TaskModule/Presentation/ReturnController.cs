using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ReturnController : ControllerBase
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<ReturnController> _logger;

        public ReturnController(ITaskDataConnection db, ILogger<ReturnController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public class ScanReturnItemRequest { public int LineId { get; set; } public string Barcode { get; set; } }
        public class ScanReturnCellRequest { public int LineId { get; set; } public string CellCode { get; set; } }

        // 1. СКАНИРОВАНИЕ ТОВАРА (Pick) - Подтверждаем, что товар в руках
        [HttpPost("assignment/{assignmentId}/scan-item")]
        public async Task<IActionResult> ScanItem(int assignmentId, [FromBody] ScanReturnItemRequest req)
        {
            var assignment = await _db.GetTable<ReturnAssignmentModel>().FirstOrDefaultAsync(a => a.Id == assignmentId);
            if (assignment == null) return NotFound("Назначение не найдено.");

            var line = await _db.GetTable<ReturnLineModel>().FirstOrDefaultAsync(l => l.Id == req.LineId && l.ReturnAssignmentId == assignmentId);
            if (line == null) return NotFound("Строка возврата не найдена.");

            if (line.ScannedQuantity >= line.Quantity) return BadRequest("Этот товар уже полностью собран.");

            // Ищем исходный товар для проверки штрих-кода
            var itemPos = await _db.GetTable<ItemPositionModel>().FirstOrDefaultAsync(ip => ip.Id == line.ItemPositionId);

            // В реальной системе тут может быть Join с таблицей Items, но если Barcode == ItemId:
            if (itemPos != null && itemPos.ItemId.ToString() == req.Barcode.Trim())
            {
                await _db.GetTable<ReturnLineModel>()
                    .Where(l => l.Id == req.LineId)
                    .Set(l => l.ScannedQuantity, l => l.ScannedQuantity + 1)
                    .UpdateAsync();

                return Ok(new { Message = "Товар собран." });
            }

            return BadRequest("Штрих-код не совпадает с ожидаемым товаром.");
        }

        // 2. СКАНИРОВАНИЕ ЯЧЕЙКИ (Place) - Распознаем строковый QR-код
        [HttpPost("assignment/{assignmentId}/scan-cell")]
        public async Task<IActionResult> ScanCell(int assignmentId, [FromBody] ScanReturnCellRequest req)
        {
            var assignment = await _db.GetTable<ReturnAssignmentModel>().FirstOrDefaultAsync(a => a.Id == assignmentId);
            if (assignment == null) return NotFound("Назначение не найдено.");

            // Ищем ячейку по текстовому коду ТОЛЬКО внутри текущего филиала
            var branchPositions = await _db.GetTable<PositionModel>()
                .Where(p => p.BranchId == assignment.BranchId)
                .ToListAsync();

            var matchedPos = branchPositions.FirstOrDefault(p => GetFullPositionCode(p) == req.CellCode.Trim());

            if (matchedPos == null) return BadRequest($"Ячейка '{req.CellCode}' не найдена на этом складе.");

            // Обновляем линию: прописываем ID целевой ячейки
            await _db.GetTable<ReturnLineModel>()
                .Where(l => l.Id == req.LineId && l.ReturnAssignmentId == assignmentId)
                .Set(l => l.TargetPositionId, matchedPos.PositionId)
                .UpdateAsync();

            return Ok(new { Message = $"Товар направлен в ячейку {req.CellCode}." });
        }

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
    }
}