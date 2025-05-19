namespace InventoryService.Application.DTOs
{
    public record LowStockItemDto
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public int CurrentQuantity { get; set; }
        public int Threshold { get; set; }
        public int DeficitQuantity => Threshold - CurrentQuantity;
    }
}
