using InventoryService.Domain.Entities;
using InventoryService.Domain.Repositories;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories
{
    public class InventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly InventoryDbContext _context;

        public InventoryTransactionRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryTransaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Inventory)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<InventoryTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Inventory)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<InventoryTransaction>> GetByInventoryIdAsync(int inventoryId, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Inventory)
                .Where(t => t.InventoryId == inventoryId)
                .ToListAsync(cancellationToken);
        }

        public async Task<InventoryTransaction> AddAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default)
        {
            await _context.InventoryTransactions.AddAsync(transaction, cancellationToken);
            return transaction;
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryTransactions.AnyAsync(t => t.Id == id, cancellationToken);
        }
    }
}
