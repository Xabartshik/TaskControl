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
    }
}