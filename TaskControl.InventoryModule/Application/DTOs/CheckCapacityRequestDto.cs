using TaskControl.Core.Shared.SharedInterfaces;

namespace TaskControl.InventoryModule.Application.DTOs
{
    public class CheckCapacityRequestDto
    {
        public int PostamatId { get; set; }
        public List<ItemToPack> ItemsToPack { get; set; } = new();
    }
}