namespace SupplierService.Application.Interfaces
{
    public interface IInventoryServiceClient
    {
        Task<bool> AddInventoryStockAsync(int productId, int quantity, string reference, CancellationToken cancellationToken = default);
    }
}
