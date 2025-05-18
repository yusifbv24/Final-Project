namespace InventoryService.Application.Events
{
    public record InventoryUpdatedEvent
    {
        public int InventoryId { get; init; }
        public int ProductId { get; init; }
        public int LocationId { get; init; }
        public int Quantity { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
