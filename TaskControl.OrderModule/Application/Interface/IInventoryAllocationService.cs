using TaskControl.Core.Shared.SharedInterfaces;
using UnitsNet;

namespace TaskControl.OrderModule.Application.Interface
{
    public interface IInventoryAllocationService
    {
        /// <summary>
        /// Ищет подходящую ячейку в постамате и резервирует её.
        /// Возвращает ID забронированной ячейки.
        /// </summary>
        Task<int> ReservePostamatCellAsync(int postamatId, List<ItemToPack> itemsToPack);
        Task<bool> HardAllocateOrderItemsAsync(int branchId, int orderPositionId, int itemId, int neededQuantity);
        Task<bool> CheckCapacityAsync(int postamatId, List<ItemToPack> itemsToPack);
    }
}