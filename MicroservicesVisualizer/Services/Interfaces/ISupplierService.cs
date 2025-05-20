using MicroservicesVisualizer.Models.Supplier;

namespace MicroservicesVisualizer.Services.Interfaces
{
    public interface ISupplierService
    {
        // Supplier methods
        Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync();
        Task<SupplierDto> GetSupplierByIdAsync(int id);
        Task<SupplierDto> GetSupplierByEmailAsync(string email);
        Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string? name, string? email, bool? isActive);
        Task<SupplierWithOrdersDto> GetSupplierWithOrdersAsync(int id);

        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto);
        Task<SupplierDto> UpdateSupplierAsync(int id, UpdateSupplierDto dto);
        Task<bool> DeleteSupplierAsync(int id);
        Task<SupplierDto> ActivateSupplierAsync(int id);
        Task<SupplierDto> DeactivateSupplierAsync(int id);

        // Purchase Order methods
        Task<IEnumerable<PurchaseOrderDto>> GetAllPurchaseOrdersAsync();
        Task<PurchaseOrderDto> GetPurchaseOrderByIdAsync(int id);
        Task<PurchaseOrderDto> GetPurchaseOrderByOrderNumberAsync(string orderNumber);
        Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersBySupplierAsync(int supplierId);
        Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersByStatusAsync(PurchaseOrderStatus status);
        Task<IEnumerable<PurchaseOrderDto>> SearchPurchaseOrdersAsync(
            int? supplierId, PurchaseOrderStatus? status,
            DateTime? startDate, DateTime? endDate,
            decimal? minAmount, decimal? maxAmount);
        Task<PurchaseOrderSummaryDto> GetPurchaseOrderSummaryAsync(DateTime? startDate, DateTime? endDate);

        Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto dto);
        Task<PurchaseOrderDto> UpdatePurchaseOrderStatusAsync(int id, UpdatePurchaseOrderStatusDto dto);
        Task<PurchaseOrderDto> ReceivePurchaseOrderItemAsync(int id, int itemId, ReceivePurchaseOrderItemDto dto);
        Task<PurchaseOrderDto> CancelPurchaseOrderAsync(int id);
    }
}
