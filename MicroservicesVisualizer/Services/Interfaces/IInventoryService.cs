using MicroservicesVisualizer.Models.Inventory;

namespace MicroservicesVisualizer.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDto>> GetAllInventoryAsync();
        Task<InventoryDto> GetInventoryByIdAsync(int id);
        Task<IEnumerable<InventoryDto>> GetInventoryByProductIdAsync(int productId);
        Task<IEnumerable<InventoryDto>> GetInventoryByLocationIdAsync(int locationId);
        Task<IEnumerable<InventoryDto>> SearchInventoryAsync(int? productId, int? locationId, int? minQuantity, int? maxQuantity, bool? isActive);
        Task<IEnumerable<LowStockItemDto>> GetLowStockItemsAsync(int threshold = 10);

        Task<IEnumerable<LocationDto>> GetAllLocationsAsync();
        Task<LocationDto> GetLocationByIdAsync(int id);
        Task<LocationDto> GetLocationByCodeAsync(string code);
        Task<IEnumerable<LocationWithInventoryDto>> GetLocationsWithInventoryAsync();

        Task<IEnumerable<InventoryTransactionDto>> GetAllTransactionsAsync();
        Task<InventoryTransactionDto> GetTransactionByIdAsync(int id);
        Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByInventoryIdAsync(int inventoryId);
        Task<IEnumerable<InventoryTransactionDto>> SearchTransactionsAsync(int? inventoryId, TransactionType? type, DateTime? startDate, DateTime? endDate);
        Task<TransactionSummaryDto> GetTransactionSummaryAsync(DateTime? startDate, DateTime? endDate);

        Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto dto);
        Task<InventoryDto> UpdateInventoryQuantityAsync(int id, UpdateInventoryDto dto);
        Task<InventoryDto> AddStockAsync(int id, AddStockRequest request);
        Task<InventoryDto> RemoveStockAsync(int id, RemoveStockRequest request);
    }
}
