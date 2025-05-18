namespace ProductService.Application.Events
{
    public record ProductCreatedEvent
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
