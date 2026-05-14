using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Application.DTOs;

namespace TaskControl.InventoryModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionCellController : ControllerBase, ICrudController<PositionCellDto, int>
    {
        private readonly IService<PositionCellDto> _service;
        private readonly ILogger<PositionCellController> _logger;

        public PositionCellController(
            IService<PositionCellDto> service,
            ILogger<PositionCellController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PositionCellDto>>> GetAll()
        {
            var records = await _service.GetAll();
            _logger.LogInformation("Получено {Count} складских позиций", records.Count());
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PositionCellDto>> GetById(int id)
        {
            var record = await _service.GetById(id);
            if (record == null)
            {
                _logger.LogWarning("Складская позиция с ID: {PositionId} не найдена", id);
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<IEnumerable<PositionCellDto>>> BulkCreate([FromBody] BulkCreatePositionDto dto)
        {
            var positionsToCreate = new List<PositionCellDto>();

            // Генерируем список DTO на основе параметров
            for (int s = 0; s < dto.StorageCount; s++)
            {
                // Добавляем ведущий ноль для красоты (например, 01, 02), если нужно
                string flsNum = (dto.StartFLSNumber + s).ToString().PadLeft(2, '0');

                if (dto.ShelvesCount == null)
                {
                    // Режим: Паллет / Одиночное место
                    positionsToCreate.Add(new PositionCellDto
                    {
                        BranchId = dto.BranchId,
                        ZoneCode = dto.ZoneCode,
                        FirstLevelStorageType = dto.StorageType,
                        FLSNumber = flsNum,
                        Status = "Active",
                        Length = dto.DefaultLength ?? 0,
                        Width = dto.DefaultWidth ?? 0,
                        Height = dto.DefaultHeight ?? 0
                    });
                }
                else
                {
                    // Режим: Стеллаж с полками и ячейками
                    for (int shelf = 1; shelf <= dto.ShelvesCount; shelf++)
                    {
                        int cellsCount = dto.CellsPerShelf ?? 1;
                        for (int cell = 1; cell <= cellsCount; cell++)
                        {
                            positionsToCreate.Add(new PositionCellDto
                            {
                                BranchId = dto.BranchId,
                                ZoneCode = dto.ZoneCode,
                                FirstLevelStorageType = dto.StorageType,
                                FLSNumber = flsNum,
                                SecondLevelStorage = shelf.ToString(),
                                ThirdLevelStorage = cell.ToString(),
                                Status = "Active",
                                Length = dto.DefaultLength ?? 0,
                                Width = dto.DefaultWidth ?? 0,
                                Height = dto.DefaultHeight ?? 0
                            });
                        }
                    }
                }
            }

            var resultList = new List<PositionCellDto>();

            // Сохраняем в БД и собираем итоговый список с ID
            foreach (var pos in positionsToCreate)
            {
                // Предполагаем, что _service.Add возвращает ID новой записи (int)
                var newId = await _service.Add(pos);

                // Используем синтаксис 'with' для создания копии с установленным ID
                resultList.Add(pos with { PositionId = newId });
            }

            _logger.LogInformation("Массовое создание завершено. Создано {Count} позиций для зоны {Zone}",
                resultList.Count, dto.ZoneCode);

            return Ok(resultList);
        }   
        [HttpPost]
        public async Task<ActionResult<int>> Add(PositionCellDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = await _service.Add(dto);
            _logger.LogInformation("Добавлена новая складская позиция. ID: {PositionId}, Зона: {Zone}", newId, dto.ZoneCode);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(PositionCellDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.Update(dto);
            if (!result)
            {
                _logger.LogWarning("Попытка обновления несуществующей складской позиции ID: {PositionId}", dto.PositionId);
                return NotFound();
            }
            _logger.LogInformation("Складская позиция ID: {PositionId} обновлена", dto.PositionId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            if (!result)
            {
                _logger.LogWarning("Попытка удаления несуществующей складской позиции ID: {PositionId}", id);
                return NotFound();
            }
            _logger.LogInformation("Складская позиция ID: {PositionId} удалена", id);
            return NoContent();
        }
    }
}