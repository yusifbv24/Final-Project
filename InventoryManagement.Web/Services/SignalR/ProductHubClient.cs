using Microsoft.AspNetCore.SignalR.Client;

namespace InventoryManagement.Web.Services.SignalR
{
    public class ProductHubClient : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly ILogger<ProductHubClient> _logger;

        public event Action<int, string>? ProductCreated;
        public event Action<int, string>? ProductUpdated;
        public event Action<int>? ProductDeleted;

        public ProductHubClient(IConfiguration configuration, ILogger<ProductHubClient> logger)
        {
            _logger = logger;

            var productServiceUrl = configuration["Services:ProductService"] ?? "http://localhost:5104";
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{productServiceUrl}/hubs/products")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<int, string>("ProductCreated", (productId, name) =>
            {
                _logger.LogInformation("Product created: {ProductId} - {Name}", productId, name);
                ProductCreated?.Invoke(productId, name);
            });

            _hubConnection.On<int, string>("ProductUpdated", (productId, name) =>
            {
                _logger.LogInformation("Product updated: {ProductId} - {Name}", productId, name);
                ProductUpdated?.Invoke(productId, name);
            });

            _hubConnection.On<int>("ProductDeleted", (productId) =>
            {
                _logger.LogInformation("Product deleted: {ProductId}", productId);
                ProductDeleted?.Invoke(productId);
            });
        }

        public async Task StartAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
                _logger.LogInformation("Connected to Product hub");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to Product hub");
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
