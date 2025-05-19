using OrderService.Domain.Entities;

namespace OrderService.Application.Events
{
    public record OrderStatusChangedEvent
    {
        public int OrderId { get; init; }
        public OrderStatus OldStatus { get; init; }
        public OrderStatus NewStatus { get; init; }
        public DateTime ChangedAt { get; init; }
    }
}
