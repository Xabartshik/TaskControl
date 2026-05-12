namespace TaskControl.InventoryModule.Application.DTOs
{
    public class AvailableItemDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal Price { get; set; }
    }
}