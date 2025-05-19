using OrderService.Domain.Entities;

namespace OrderService.Application.Events
{
    public record OrderCreatedEvent
    {
        public int OrderId { get; init; }
        public int CustomerId { get; init; }
        public OrderStatus Status { get; init; }
        public decimal TotalAmount { get; init; }
        public DateTime OrderDate { get; init; }
        public IEnumerable<OrderItemEvent> Items { get; init; } = new List<OrderItemEvent>();
    }

    public record OrderItemEvent
    {
        public int ProductId { get; init; }
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
