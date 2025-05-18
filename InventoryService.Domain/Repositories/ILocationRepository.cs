using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Repositories
{
    public interface ILocationRepository
    {
        Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Location>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Location?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<Location> AddAsync(Location location, CancellationToken cancellationToken = default);
        Task UpdateAsync(Location location, CancellationToken cancellationToken = default);
        Task DeleteAsync(Location location, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    }
}
