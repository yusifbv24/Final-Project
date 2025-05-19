using SupplierService.Domain.Entities;

namespace SupplierService.Domain.Repositories
{
    public interface ISupplierRepository
    {
        Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Supplier?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
        Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);
        Task DeleteAsync(Supplier supplier, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
