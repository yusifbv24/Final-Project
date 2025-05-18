namespace InventoryService.Application.Interfaces
{
    public interface IProductServiceClient
    {
        Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default);
        Task<string> GetProductNameAsync(int productId, CancellationToken cancellationToken = default);
    }
}
