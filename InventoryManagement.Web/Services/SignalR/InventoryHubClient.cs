using Microsoft.AspNetCore.SignalR.Client;

namespace InventoryManagement.Web.Services.SignalR
{
    public class InventoryHubClient : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly ILogger<InventoryHubClient> _logger;

        public event Action<int, int, int>? InventoryUpdated;
        public event Action<int, int, int, string, int>? InventoryTransactionCreated;
        public event Action<int, int, int, int, int>? LowStockAlert;

        public InventoryHubClient(IConfiguration configuration, ILogger<InventoryHubClient> logger)
        {
            _logger = logger;

            var inventoryServiceUrl = configuration["Services:InventoryService"] ?? "http://localhost:5105";
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{inventoryServiceUrl}/hubs/inventory")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<int, int, int>("InventoryUpdated", (inventoryId, productId, quantity) =>
            {
                _logger.LogInformation("Inventory updated: {InventoryId} - Product {ProductId} - Quantity {Quantity}",
                    inventoryId, productId, quantity);
                InventoryUpdated?.Invoke(inventoryId, productId, quantity);
            });

            _hubConnection.On<int, int, int, string, int>("InventoryTransactionCreated",
                (transactionId, inventoryId, productId, type, quantity) =>
                {
                    _logger.LogInformation("Inventory transaction created: {TransactionId} - Inventory {InventoryId} - Type {Type} - Quantity {Quantity}",
                        transactionId, inventoryId, type, quantity);
                    InventoryTransactionCreated?.Invoke(transactionId, inventoryId, productId, type, quantity);
                });

            _hubConnection.On<int, int, int, int, int>("LowStockAlert",
                (inventoryId, productId, locationId, quantity, threshold) =>
                {
                    _logger.LogWarning("Low stock alert: Product {ProductId} - Location {LocationId} - Quantity {Quantity}/{Threshold}",
                        productId, locationId, quantity, threshold);
                    LowStockAlert?.Invoke(inventoryId, productId, locationId, quantity, threshold);
                });
        }

        public async Task StartAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
                _logger.LogInformation("Connected to Inventory hub");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to Inventory hub");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
