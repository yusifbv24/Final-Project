using OrderService.Domain.Entities;

namespace OrderService.Application.DTOs
{
    public record UpdateOrderStatusDto
    {
        public OrderStatus Status { get; init; }
    }
}
