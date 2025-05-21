namespace OrderService.Application.DTOs
{
    public record CreateOrderDto
    {
        public string CustomerName { get; init; } = string.Empty;
        public string CustomerEmail { get; init; } = string.Empty;
        public string ShippingAddress { get; init; } = string.Empty;
        public List<OrderItemDto> Items { get; init; } = new List<OrderItemDto>();
    }
}
