using InventoryService.Domain.Entities;
using InventoryService.Domain.Repositories;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly InventoryDbContext _context;

        public InventoryRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Inventory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .Include(i => i.Location)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .Include(i => i.Location)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Inventory>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .Include(i => i.Location)
                .Where(i => i.ProductId == productId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Inventory>> GetByLocationIdAsync(int locationId, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .Include(i => i.Location)
                .Where(i => i.LocationId == locationId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Inventory?> GetByProductAndLocationAsync(int productId, int locationId, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .Include(i => i.Location)
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.LocationId == locationId, cancellationToken);
        }

        public async Task<Inventory> AddAsync(Inventory inventory, CancellationToken cancellationToken = default)
        {
            await _context.Inventories.AddAsync(inventory, cancellationToken);
            return inventory;
        }

        public Task UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default)
        {
            _context.Inventories.Update(inventory);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Inventory inventory, CancellationToken cancellationToken = default)
        {
            _context.Inventories.Remove(inventory);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories.AnyAsync(i => i.Id == id, cancellationToken);
        }
    }
}
