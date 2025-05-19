namespace OrderService.Application.DTOs
{
    public record CreateOrderDto
    {
        public int CustomerId { get; init; }
        public string ShippingAddress { get; init; } = string.Empty;
        public string BillingAddress { get; init; } = string.Empty;
        public string? Notes { get; init; }
        public IEnumerable<CreateOrderItemDto>? Items { get; init; }
    }
}
