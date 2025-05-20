namespace MicroservicesVisualizer.Models.Inventory
{
    public class LowStockItemDto
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int Threshold { get; set; }
    }
}
