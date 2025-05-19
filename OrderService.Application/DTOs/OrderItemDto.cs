namespace OrderService.Application.DTOs
{
    public record OrderItemDto
    {
        public int Id { get; init; }
        public int OrderId { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}