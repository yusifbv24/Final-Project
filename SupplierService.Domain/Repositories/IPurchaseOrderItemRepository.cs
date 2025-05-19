using SupplierService.Domain.Entities;

namespace SupplierService.Domain.Repositories
{
    public interface IPurchaseOrderItemRepository
    {
        Task<PurchaseOrderItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<PurchaseOrderItem>> GetByPurchaseOrderIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
        Task<PurchaseOrderItem> AddAsync(PurchaseOrderItem purchaseOrderItem, CancellationToken cancellationToken = default);
        Task UpdateAsync(PurchaseOrderItem purchaseOrderItem, CancellationToken cancellationToken = default);
        Task DeleteAsync(PurchaseOrderItem purchaseOrderItem, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
