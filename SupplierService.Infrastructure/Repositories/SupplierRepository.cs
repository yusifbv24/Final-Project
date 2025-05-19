using Microsoft.EntityFrameworkCore;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Repositories;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly SupplierDbContext _context;

        public SupplierRepository(SupplierDbContext context)
        {
            _context = context;
        }

        public async Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.ToListAsync(cancellationToken);
        }

        public async Task<Supplier?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Email == email, cancellationToken);
        }

        public async Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            await _context.Suppliers.AddAsync(supplier, cancellationToken);
            return supplier;
        }

        public Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            _context.Suppliers.Update(supplier);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            _context.Suppliers.Remove(supplier);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.AnyAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.AnyAsync(s => s.Email == email, cancellationToken);
        }
    }
}