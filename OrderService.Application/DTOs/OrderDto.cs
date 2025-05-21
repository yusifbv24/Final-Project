using OrderService.Domain.Entities;

namespace OrderService.Application.DTOs
{
    public record OrderDto
    {
        public int Id { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string CustomerEmail { get; init; } = string.Empty;
        public string ShippingAddress { get; init; } = string.Empty;
        public OrderStatus Status { get; init; }
        public decimal TotalAmount { get; init; }
        public DateTime OrderDate { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public List<OrderItemDto> Items { get; init; } = new List<OrderItemDto>();
    }

}
