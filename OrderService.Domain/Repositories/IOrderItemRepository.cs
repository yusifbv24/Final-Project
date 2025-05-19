using OrderService.Domain.Entities;

namespace OrderService.Domain.Repositories
{
    public interface IOrderItemRepository
    {
        Task<OrderItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
        Task<OrderItem> AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
        Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
        Task DeleteAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}