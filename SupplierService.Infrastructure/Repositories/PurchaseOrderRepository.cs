using Microsoft.EntityFrameworkCore;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Repositories;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly SupplierDbContext _context;

        public PurchaseOrderRepository(SupplierDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.OrderNumber == orderNumber, cancellationToken);
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<PurchaseOrder>> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                .Where(p => p.SupplierId == supplierId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                .Where(p => p.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<PurchaseOrder> AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
        {
            await _context.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
            return purchaseOrder;
        }

        public Task UpdateAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
        {
            _context.PurchaseOrders.Update(purchaseOrder);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
        {
            _context.PurchaseOrders.Remove(purchaseOrder);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders.AnyAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders.AnyAsync(p => p.OrderNumber == orderNumber, cancellationToken);
        }
    }
}
