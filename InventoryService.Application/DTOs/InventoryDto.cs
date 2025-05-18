namespace InventoryService.Application.DTOs
{
    public record InventoryDto
    {
        public int Id { get; init; }
        public int ProductId { get; init; }
        public int LocationId { get; init; }
        public string? LocationName { get; init; }
        public int Quantity { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
