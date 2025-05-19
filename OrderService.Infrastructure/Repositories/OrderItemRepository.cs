using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly OrderDbContext _context;

        public OrderItemRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<OrderItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.OrderItems
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.OrderItems
                .Where(i => i.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }

        public async Task<OrderItem> AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            await _context.OrderItems.AddAsync(orderItem, cancellationToken);
            return orderItem;
        }

        public Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            _context.OrderItems.Update(orderItem);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            _context.OrderItems.Remove(orderItem);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.OrderItems.AnyAsync(i => i.Id == id, cancellationToken);
        }
    }
}
