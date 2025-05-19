namespace InventoryService.Application.DTOs
{
    public record LocationWithInventoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalItems { get; set; }
        public int TotalUniqueProducts { get; set; }
        public int TotalQuantity { get; set; }
    }
}