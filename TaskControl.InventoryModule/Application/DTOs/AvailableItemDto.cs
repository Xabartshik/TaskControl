namespace TaskControl.InventoryModule.Application.DTOs
{
    public class AvailableItemDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Сведения о количестве филиалов и суммарном остатке товара.
    /// </summary>
    public class ItemStockDto
    {
        public int BranchCount { get; set; }
        public int TotalAvailableQuantity { get; set; }
    }

    /// <summary>
    /// Доступный остаток товара в конкретном филиале.
    /// </summary>
    public class BranchStockDto
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
    }
}