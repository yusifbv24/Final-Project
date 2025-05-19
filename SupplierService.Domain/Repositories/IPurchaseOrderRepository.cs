using SupplierService.Domain.Entities;

namespace SupplierService.Domain.Repositories
{
    public interface IPurchaseOrderRepository
    {
        Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<PurchaseOrder>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<PurchaseOrder>> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status, CancellationToken cancellationToken = default);
        Task<PurchaseOrder> AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
        Task UpdateAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
        Task DeleteAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    }
}
