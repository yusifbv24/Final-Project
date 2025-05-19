using OrderService.Domain.Entities;

namespace OrderService.Application.DTOs
{
    public record OrderDto
    {
        public int Id { get; init; }
        public int CustomerId { get; init; }
        public string? CustomerName { get; init; }
        public OrderStatus Status { get; init; }
        public decimal TotalAmount { get; init; }
        public string ShippingAddress { get; init; } = string.Empty;
        public string BillingAddress { get; init; } = string.Empty;
        public string? Notes { get; init; }
        public DateTime OrderDate { get; init; }
        public DateTime? ShippedDate { get; init; }
        public DateTime? DeliveredDate { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public IEnumerable<OrderItemDto>? Items { get; init; }
    }
}
