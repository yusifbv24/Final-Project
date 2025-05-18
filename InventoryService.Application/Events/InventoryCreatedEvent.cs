namespace InventoryService.Application.Events
{
    public record InventoryCreatedEvent
    {
        public int InventoryId { get; init; }
        public int ProductId { get; init; }
        public int LocationId { get; init; }
        public int Quantity { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}