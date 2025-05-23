using InventoryManagement.Web.Services.RabbitMQ;
using Microsoft.AspNetCore.SignalR;

namespace InventoryManagement.Web.Services.SignalR
{
    public class ProductHubProxy : Hub
    {
        private readonly ProductHubClient _productHubClient;
        private readonly RabbitMQListener _rabbitMQListener;
        private readonly ILogger<ProductHubProxy> _logger;

        public ProductHubProxy(
            ProductHubClient productHubClient,
            RabbitMQListener rabbitMQListener,
            ILogger<ProductHubProxy> logger)
        {
            _productHubClient = productHubClient;
            _rabbitMQListener = rabbitMQListener;
            _logger = logger;

            // Forward SignalR events
            _productHubClient.ProductCreated += async (productId, name) =>
            {
                await Clients.All.SendAsync("ProductCreated", productId, name);
            };

            _productHubClient.ProductUpdated += async (productId, name) =>
            {
                await Clients.All.SendAsync("ProductUpdated", productId, name);
            };

            _productHubClient.ProductDeleted += async (productId) =>
            {
                await Clients.All.SendAsync("ProductDeleted", productId);
            };

            // Forward RabbitMQ events
            _rabbitMQListener.MessageReceived += async (routingKey, message) =>
            {
                if (routingKey == "product.created" || routingKey == "product.updated")
                {
                    await Clients.All.SendAsync("MessageReceived", routingKey, message);
                }
            };
        }
    }

    public class InventoryHubProxy : Hub
    {
        private readonly InventoryHubClient _inventoryHubClient;
        private readonly RabbitMQListener _rabbitMQListener;
        private readonly ILogger<InventoryHubProxy> _logger;

        public InventoryHubProxy(
            InventoryHubClient inventoryHubClient,
            RabbitMQListener rabbitMQListener,
            ILogger<InventoryHubProxy> logger)
        {
            _inventoryHubClient = inventoryHubClient;
            _rabbitMQListener = rabbitMQListener;
            _logger = logger;

            // Forward SignalR events
            _inventoryHubClient.InventoryUpdated += async (inventoryId, productId, quantity) =>
            {
                await Clients.All.SendAsync("InventoryUpdated", inventoryId, productId, quantity);
            };

            _inventoryHubClient.InventoryTransactionCreated += async (transactionId, inventoryId, productId, type, quantity) =>
            {
                await Clients.All.SendAsync("InventoryTransactionCreated", transactionId, inventoryId, productId, type, quantity);
            };

            _inventoryHubClient.LowStockAlert += async (inventoryId, productId, locationId, quantity, threshold) =>
            {
                await Clients.All.SendAsync("LowStockAlert", inventoryId, productId, locationId, quantity, threshold);
            };

            // Forward RabbitMQ events
            _rabbitMQListener.MessageReceived += async (routingKey, message) =>
            {
                if (routingKey.StartsWith("inventory."))
                {
                    await Clients.All.SendAsync("MessageReceived", routingKey, message);
                }
            };
        }
    }

    public class OrderHubProxy : Hub
    {
        private readonly OrderHubClient _orderHubClient;
        private readonly RabbitMQListener _rabbitMQListener;
        private readonly ILogger<OrderHubProxy> _logger;

        public OrderHubProxy(
            OrderHubClient orderHubClient,
            RabbitMQListener rabbitMQListener,
            ILogger<OrderHubProxy> logger)
        {
            _orderHubClient = orderHubClient;
            _rabbitMQListener = rabbitMQListener;
            _logger = logger;

            // Ensure connection is established before setting up event handlers
            Task.Run(async () =>
            {
                await _orderHubClient.StartAsync();
                SetupEventHandlers();
            });
        }

        private void SetupEventHandlers()
        {
            // Forward SignalR events from OrderHubClient
            _orderHubClient.OrderCreated += async (orderId, customerName) =>
            {
                try
                {
                    _logger.LogInformation("Forwarding OrderCreated event: {OrderId} - {CustomerName}", orderId, customerName);
                    await Clients.All.SendAsync("OrderCreated", orderId, customerName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error forwarding OrderCreated event");
                }
            };

            _orderHubClient.OrderStatusChanged += async (orderId, status) =>
            {
                try
                {
                    _logger.LogInformation("Forwarding OrderStatusChanged event: {OrderId} - {Status}", orderId, status);
                    await Clients.All.SendAsync("OrderStatusChanged", orderId, status);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error forwarding OrderStatusChanged event");
                }
            };

            // Forward RabbitMQ events
            _rabbitMQListener.MessageReceived += async (routingKey, message) =>
            {
                if (routingKey.StartsWith("order."))
                {
                    try
                    {
                        _logger.LogInformation("Forwarding RabbitMQ message: {RoutingKey}", routingKey);
                        await Clients.All.SendAsync("MessageReceived", routingKey, message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error forwarding RabbitMQ message");
                    }
                }
            };
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to OrderHubProxy: {ConnectionId}", Context.ConnectionId);

            // Ensure the OrderHubClient is connected
            if (!_orderHubClient.IsConnected)
            {
                await _orderHubClient.StartAsync();
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from OrderHubProxy: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
