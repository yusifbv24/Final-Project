using SupplierService.Domain.Repositories;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SupplierDbContext _context;

        public UnitOfWork(SupplierDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
