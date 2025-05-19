using SupplierService.Domain.Entities;

namespace SupplierService.Application.DTOs
{
    public record UpdatePurchaseOrderStatusDto
    {
        public PurchaseOrderStatus Status { get; init; }
    }
}
