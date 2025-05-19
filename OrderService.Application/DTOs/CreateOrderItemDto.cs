namespace OrderService.Application.DTOs
{
    public record CreateOrderItemDto
    {
        public int ProductId { get; init; }
        public int Quantity { get; init; }
    }
}