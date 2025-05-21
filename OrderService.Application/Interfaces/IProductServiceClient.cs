namespace OrderService.Application.Interfaces
{
    public record ProductInfo(int Id, string Name, decimal Price, bool IsAvailable);

    public interface IProductServiceClient
    {
        Task<ProductInfo?> GetProductAsync(int productId, CancellationToken cancellationToken = default);
        Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default);
    }
}
