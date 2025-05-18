namespace InventoryService.Application.DTOs
{
    public record UpdateLocationDto
    {
        public string Name { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }
}