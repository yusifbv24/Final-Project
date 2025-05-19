using Microsoft.EntityFrameworkCore;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Repositories;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories
{
    public class PurchaseOrderItemRepository : IPurchaseOrderItemRepository
    {
        private readonly SupplierDbContext _context;

        public PurchaseOrderItemRepository(SupplierDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseOrderItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrderItems
                .Include(i => i.PurchaseOrder)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<PurchaseOrderItem>> GetByPurchaseOrderIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrderItems
                .Where(i => i.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);
        }

        public async Task<PurchaseOrderItem> AddAsync(PurchaseOrderItem purchaseOrderItem, CancellationToken cancellationToken = default)
        {
            await _context.PurchaseOrderItems.AddAsync(purchaseOrderItem, cancellationToken);
            return purchaseOrderItem;
        }

        public Task UpdateAsync(PurchaseOrderItem purchaseOrderItem, CancellationToken cancellationToken = default)
        {
            _context.PurchaseOrderItems.Update(purchaseOrderItem);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(PurchaseOrderItem purchaseOrderItem, CancellationToken cancellationToken = default)
        {
            _context.PurchaseOrderItems.Remove(purchaseOrderItem);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrderItems.AnyAsync(i => i.Id == id, cancellationToken);
        }
    }
}
