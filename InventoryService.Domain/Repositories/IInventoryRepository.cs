using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Inventory>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Inventory>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Inventory>> GetByLocationIdAsync(int locationId, CancellationToken cancellationToken = default);
        Task<Inventory?> GetByProductAndLocationAsync(int productId, int locationId, CancellationToken cancellationToken = default);
        Task<Inventory> AddAsync(Inventory inventory, CancellationToken cancellationToken = default);
        Task UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default);
        Task DeleteAsync(Inventory inventory, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
