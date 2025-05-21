using OrderService.Domain.Entities;

namespace OrderService.Application.Events
{
    public record OrderCreatedEvent
    {
        public int OrderId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public OrderStatus Status { get; init; }
        public decimal TotalAmount { get; init; }
        public DateTime OrderDate { get; init; }
        public IEnumerable<OrderItemDetail> Items { get; init; } = new List<OrderItemDetail>();

        public record OrderItemDetail(int ProductId, int Quantity);
    }
}
