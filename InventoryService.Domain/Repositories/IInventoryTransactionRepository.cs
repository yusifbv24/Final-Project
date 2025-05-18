using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Repositories
{
    public interface IInventoryTransactionRepository
    {
        Task<InventoryTransaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryTransaction>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryTransaction>> GetByInventoryIdAsync(int inventoryId, CancellationToken cancellationToken = default);
        Task<InventoryTransaction> AddAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
