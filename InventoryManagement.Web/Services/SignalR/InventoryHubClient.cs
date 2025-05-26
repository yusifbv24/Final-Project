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
        private bool _isConnected = false;
        public bool IsConnected => _isConnected && _hubConnection.State == HubConnectionState.Connected;

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

            _hubConnection.Closed += async (error) => {
                _isConnected = false;
                _logger.LogWarning("Connection to Inventory hub closed. Error: {Error}", error?.Message);
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await StartAsync();
            };

            _hubConnection.Reconnected += (connectionId) => {
                _isConnected = true;
                _logger.LogInformation("Reconnected to Inventory hub. ConnectionId: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            _hubConnection.Reconnecting += (error) => {
                _isConnected = false;
                _logger.LogWarning("Reconnecting to Inventory hub. Error: {Error}", error?.Message);
                return Task.CompletedTask;
            };
        }

        public async Task StartAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    _isConnected = true;
                    _logger.LogInformation("Connected to Inventory hub");
                }
                catch (Exception ex)
                {
                    _isConnected = false;
                    _logger.LogError(ex, "Error connecting to Inventory hub");
                    // Retry after 5 seconds
                    await Task.Delay(5000);
                    await StartAsync();
                }
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
