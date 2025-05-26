using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ProductService.Application.Hubs
{
    public class ProductHub : Hub
    {
        private readonly ILogger<ProductHub> _logger;

        public ProductHub(ILogger<ProductHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to ProductHub: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from ProductHub: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // Method to send product created event
        public async Task NotifyProductCreated(int productId, string productName)
        {
            await Clients.All.SendAsync("ProductCreated", productId, productName);
            _logger.LogInformation("ProductCreated event sent: {ProductId} - {ProductName}", productId, productName);
        }

        // Method to send product updated event
        public async Task NotifyProductUpdated(int productId, string productName)
        {
            await Clients.All.SendAsync("ProductUpdated", productId, productName);
            _logger.LogInformation("ProductUpdated event sent: {ProductId} - {ProductName}", productId, productName);
        }

        // Method to send product deleted event
        public async Task NotifyProductDeleted(int productId)
        {
            await Clients.All.SendAsync("ProductDeleted", productId);
            _logger.LogInformation("ProductDeleted event sent: {ProductId}", productId);
        }
    }
}
