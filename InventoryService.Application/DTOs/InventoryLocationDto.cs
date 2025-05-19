namespace InventoryService.Application.DTOs
{
    public record InventoryLocationDto
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
