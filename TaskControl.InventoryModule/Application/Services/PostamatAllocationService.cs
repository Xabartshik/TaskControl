// TaskControl.InventoryModule/Application/Services/PostamatAllocationService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.Domain;
using TaskControl.OrderModule.Application.Interface;


namespace TaskControl.InventoryModule.Application.Services
{
    public class PostamatAllocationService : IPostamatAllocationService
    {
        private readonly IPostamatCellRepository _cellRepository;
        private readonly IBoxPackingService _packingService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<PostamatAllocationService> _logger;

        public PostamatAllocationService(
            IPostamatCellRepository cellRepository,
            IBoxPackingService packingService,
            IOptions<AppSettings> appSettings,
            ILogger<PostamatAllocationService> logger)
        {
            _cellRepository = cellRepository;
            _packingService = packingService;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        // Приватный метод грубой проверки
        private bool PassesRoughDimensionCheck(List<ItemToPack> itemsToPack)
        {
            double totalWeight = 0;

            foreach (var item in itemsToPack)
            {
                // Если товар вращать можно, логичнее сортировать измерения товара и ячейки.
                // Для простейшей проверки предполагаем, что у товара (L, W, H) ни одно измерение 
                // не должно превышать абсолютный максимум постамата (L).
                // Упрощенный вариант проверки (сортируем грани товара по убыванию: Max, Mid, Min)
                var dimensions = new[] { item.Length, item.Width, item.Height }.OrderByDescending(x => x).ToArray();
                var maxLimits = new[] { _appSettings.PostamatMaxItemLengthMm, _appSettings.PostamatMaxItemWidthMm, _appSettings.PostamatMaxItemHeightMm }.OrderByDescending(x => x).ToArray();

                if (dimensions[0] > maxLimits[0] || dimensions[1] > maxLimits[1] || dimensions[2] > maxLimits[2])
                {
                    return false; // Товар физически не пролезет ни в одну ячейку
                }

                // Можно также суммировать вес, если у ItemToPack есть свойство Weight
                totalWeight += item.Weight * item.Quantity;
            }

            if (totalWeight > _appSettings.PostamatMaxWeightGrams) return false;

            return true;
        }

        public async Task<bool> CheckCapacityAsync(int postamatId, List<ItemToPack> itemsToPack)
        {
            // 1. Быстрая (грубая) проверка габаритов ДО запроса к БД
            if (!PassesRoughDimensionCheck(itemsToPack))
            {
                _logger.LogWarning("Заказ отклонен: содержит товары, превышающие максимально допустимые габариты постамата.");
                return false;
            }
            var availableCells = await _cellRepository.GetAvailableCellsAsync(postamatId);
            if (!availableCells.Any()) return false;

            // Сортируем ячейки от меньшей к большей
            var sortedCells = availableCells.OrderBy(c => c.Capacity).ToList();

            foreach (var cell in sortedCells)
            {
                var cellToPack = new List<CellToPackInto>
        {
            new CellToPackInto
            {
                PositionId = cell.CellId,
                Length = cell.Length.Millimeters,
                Width = cell.Width.Millimeters,
                Height = cell.Height.Millimeters
            }
        };

                var packingResult = _packingService.AssignItemsToPickupCells(itemsToPack, cellToPack);

                // Если алгоритм смог разместить ВСЕ товары в эту ячейку, вместимость подтверждена
                if (packingResult.IsFullyPacked)
                {
                    return true;
                }
            }

            return false; // Нет подходящей ячейки
        }

        public async Task<int> ReservePostamatCellAsync(int postamatId, List<ItemToPack> itemsToPack)
        {
            var availableCells = await _cellRepository.GetAvailableCellsAsync(postamatId);

            // Оптимизация: сортируем по объему, чтобы сначала занимать самые маленькие ячейки
            var sortedCells = availableCells.OrderBy(c => c.Capacity).ToList();

            foreach (var cell in sortedCells)
            {
                // Эмулируем одну ячейку постамата как цель для упаковки
                var cellToPack = new List<CellToPackInto>
                {
                    new CellToPackInto
                    {
                        PositionId = cell.CellId,
                        Length = cell.Length.Millimeters,
                        Width = cell.Width.Millimeters,
                        Height = cell.Height.Millimeters
                    }
                };

                // Прогоняем через ваш алгоритм упаковки
                var packingResult = _packingService.AssignItemsToPickupCells(itemsToPack, cellToPack);

                if (packingResult.IsFullyPacked)
                {
                    _logger.LogInformation("Алгоритм успешно подобрал ячейку {CellId} для постамата {PostamatId}", cell.CellId, postamatId);

                    cell.Status = PostamatCellStatus.Reserved;
                    await _cellRepository.UpdateAsync(cell);

                    return cell.CellId;
                }
            }

            _logger.LogWarning("ОТКАЗ: Ни одна свободная ячейка в постамате {PostamatId} не вмещает заказ.", postamatId);
            return -1;
        }
    }
}