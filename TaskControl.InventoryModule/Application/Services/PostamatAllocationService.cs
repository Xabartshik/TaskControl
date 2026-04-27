// TaskControl.InventoryModule/Application/Services/PostamatAllocationService.cs
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.Domain;
using TaskControl.OrderModule.Application.Interface;


namespace TaskControl.InventoryModule.Application.Services
{
    public class PostamatAllocationService : IInventoryAllocationService
    {
        private readonly IPostamatCellRepository _cellRepository;
        private readonly IBoxPackingService _packingService;
        private readonly ILogger<PostamatAllocationService> _logger;

        public PostamatAllocationService(
            IPostamatCellRepository cellRepository,
            IBoxPackingService packingService,
            ILogger<PostamatAllocationService> logger)
        {
            _cellRepository = cellRepository;
            _packingService = packingService;
            _logger = logger;
        }
        //TODO: Заглушка. Потом разделить
        public Task<bool> HardAllocateOrderItemsAsync(int branchId, int orderPositionId, int itemId, int neededQuantity)
        {
            throw new NotImplementedException();
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