using InventoryManagement.Web.Services.RabbitMQ;
using Microsoft.AspNetCore.SignalR;

namespace InventoryManagement.Web.Services.SignalR
{
    public class ProductHubProxy : Hub
    {
        private readonly ProductHubClient _productHubClient;
        private readonly RabbitMQListener _rabbitMQListener;
        private readonly ILogger<ProductHubProxy> _logger;
        private static readonly object _lock = new object();
        private static bool _handlersRegistered = false;

        public ProductHubProxy(
            ProductHubClient productHubClient,
            RabbitMQListener rabbitMQListener,
            ILogger<ProductHubProxy> logger)
        {
            _productHubClient = productHubClient;
            _rabbitMQListener = rabbitMQListener;
            _logger = logger;

            RegisterHandlers();
        }
        private void RegisterHandlers()
        {
            lock (_lock)
            {
                if (!_handlersRegistered)
                {
                    _productHubClient.ProductCreated += async (productId, name) =>
                    {
                        await HandleProductCreated(productId, name);
                    };

                    _productHubClient.ProductUpdated += async (productId, name) =>
                    {
                        await HandleProductUpdated(productId, name);
                    };

                    _productHubClient.ProductDeleted += async (productId) =>
                    {
                        await HandleProductDeleted(productId);
                    };

                    _rabbitMQListener.MessageReceived += async (routingKey, message) =>
                    {
                        if (routingKey.StartsWith("product."))
                        {
                            await HandleRabbitMQMessage(routingKey, message);
                        }
                    };

                    _handlersRegistered = true;
                }
            }
        }
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to ProductHub: {ConnectionId}", Context.ConnectionId);

            // Set up event handlers for this connection
            _productHubClient.ProductCreated += ProductCreatedHandler;
            _productHubClient.ProductUpdated += ProductUpdatedHandler;
            _productHubClient.ProductDeleted += ProductDeletedHandler;
            _rabbitMQListener.MessageReceived += RabbitMQMessageHandler;

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from ProductHub: {ConnectionId}", Context.ConnectionId);

            // Clean up event handlers
            _productHubClient.ProductCreated -= ProductCreatedHandler;
            _productHubClient.ProductUpdated -= ProductUpdatedHandler;
            _productHubClient.ProductDeleted -= ProductDeletedHandler;
            _rabbitMQListener.MessageReceived -= RabbitMQMessageHandler;

            await base.OnDisconnectedAsync(exception);
        }

        private async void ProductCreatedHandler(int productId, string name)
        {
            try
            {
                await Clients.All.SendAsync("ProductCreated", productId, name);
                _logger.LogInformation("Forwarded ProductCreated: {ProductId} - {Name}", productId, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding ProductCreated");
            }
        }

        private async void ProductUpdatedHandler(int productId, string name)
        {
            try
            {
                await Clients.All.SendAsync("ProductUpdated", productId, name);
                _logger.LogInformation("Forwarded ProductUpdated: {ProductId} - {Name}", productId, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding ProductUpdated");
            }
        }

        private async void ProductDeletedHandler(int productId)
        {
            try
            {
                await Clients.All.SendAsync("ProductDeleted", productId);
                _logger.LogInformation("Forwarded ProductDeleted: {ProductId}", productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding ProductDeleted");
            }
        }

        private async void RabbitMQMessageHandler(string routingKey, string message)
        {
            if (routingKey.StartsWith("product."))
            {
                try
                {
                    await Clients.All.SendAsync("MessageReceived", routingKey, message);
                    _logger.LogInformation("Forwarded RabbitMQ message: {RoutingKey}", routingKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error forwarding RabbitMQ message");
                }
            }
        }

        private async Task HandleProductCreated(int productId, string name)
        {
            try
            {
                await Clients.All.SendAsync("ProductCreated", productId, name);
                _logger.LogInformation("Forwarded ProductCreated: {ProductId} - {Name}", productId, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding ProductCreated");
            }
        }

        private async Task HandleProductUpdated(int productId, string name)
        {
            try
            {
                await Clients.All.SendAsync("ProductUpdated", productId, name);
                _logger.LogInformation("Forwarded ProductUpdated: {ProductId} - {Name}", productId, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding ProductUpdated");
            }
        }

        private async Task HandleProductDeleted(int productId)
        {
            try
            {
                await Clients.All.SendAsync("ProductDeleted", productId);
                _logger.LogInformation("Forwarded ProductDeleted: {ProductId}", productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding ProductDeleted");
            }
        }

        private async Task HandleRabbitMQMessage(string routingKey, string message)
        {
            try
            {
                await Clients.All.SendAsync("MessageReceived", routingKey, message);
                _logger.LogInformation("Forwarded RabbitMQ message: {RoutingKey}", routingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding RabbitMQ message");
            }
        }
    }
}
