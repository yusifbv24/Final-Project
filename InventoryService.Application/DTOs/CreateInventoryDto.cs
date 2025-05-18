namespace InventoryService.Application.DTOs
{
    public record CreateInventoryDto
    {
        public int ProductId { get; init; }
        public int LocationId { get; init; }
        public int Quantity { get; init; }
    }
}
