namespace InventoryService.Application.DTOs
{
    public record UpdateInventoryDto
    {
        public int Quantity { get; init; }
    }
}