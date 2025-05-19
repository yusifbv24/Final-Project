namespace OrderService.Application.Interfaces
{
    public record ProductInfo(int Id, string Name, decimal Price);

    public interface IProductServiceClient
    {
        Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default);
        Task<ProductInfo?> GetProductAsync(int productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductInfo>> GetProductsAsync(IEnumerable<int> productIds, CancellationToken cancellationToken = default);
    }
}
