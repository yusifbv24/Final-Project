using InventoryService.Domain.Entities;
using InventoryService.Domain.Repositories;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly InventoryDbContext _context;

        public LocationRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Locations.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<Location>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Locations.ToListAsync(cancellationToken);
        }

        public async Task<Location?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Locations
                .FirstOrDefaultAsync(l => l.Code == code, cancellationToken);
        }

        public async Task<Location> AddAsync(Location location, CancellationToken cancellationToken = default)
        {
            await _context.Locations.AddAsync(location, cancellationToken);
            return location;
        }

        public Task UpdateAsync(Location location, CancellationToken cancellationToken = default)
        {
            _context.Locations.Update(location);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Location location, CancellationToken cancellationToken = default)
        {
            _context.Locations.Remove(location);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Locations.AnyAsync(l => l.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Locations.AnyAsync(l => l.Code == code, cancellationToken);
        }
    }
}