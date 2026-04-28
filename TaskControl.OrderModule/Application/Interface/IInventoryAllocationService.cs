using TaskControl.Core.Shared.SharedInterfaces;

namespace TaskControl.OrderModule.Application.Interface
{
    // Интерфейс для работы с товарами на полках
    public interface IItemAllocationService
    {
        Task<bool> HardAllocateOrderItemsAsync(int branchId, int orderPositionId, int itemId, int neededQuantity);
    }

    // Интерфейс для работы с постаматами
    public interface IPostamatAllocationService
    {
        Task<int> ReservePostamatCellAsync(int postamatId, List<ItemToPack> itemsToPack);
        Task<bool> CheckCapacityAsync(int postamatId, List<ItemToPack> itemsToPack);
    }
}