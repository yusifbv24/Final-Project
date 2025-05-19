namespace OrderService.Application.Interfaces
{
    public interface IInventoryServiceClient
    {
        Task<bool> CheckStockAvailabilityAsync(int productId, int quantity, CancellationToken cancellationToken = default);
        Task<bool> ReserveStockAsync(int productId, int quantity, string reference, CancellationToken cancellationToken = default);
        Task<bool> ReleaseStockReservationAsync(int productId, int quantity, string reference, CancellationToken cancellationToken = default);
    }
}
