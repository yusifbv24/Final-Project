namespace OrderService.Application.DTOs
{
    public record UpdateOrderItemDto
    {
        public int Quantity { get; init; }
    }
}