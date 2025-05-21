using System.Text.Json;
using System.Text;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace InventoryManagement.Web.Services.RabbitMQ
{
    public class RabbitMQListener : BackgroundService
    {
        private readonly ILogger<RabbitMQListener> _logger;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IModel? _channel;

        public event Action<string, string>? MessageReceived;

        public RabbitMQListener(
            ILogger<RabbitMQListener> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var rabbitMQConfig = _configuration.GetSection("RabbitMQ");
                var factory = new ConnectionFactory
                {
                    HostName = rabbitMQConfig["Host"] ?? "localhost",
                    Port = int.Parse(rabbitMQConfig["Port"] ?? "5672"),
                    UserName = rabbitMQConfig["Username"] ?? "guest",
                    Password = rabbitMQConfig["Password"] ?? "guest",
                    VirtualHost = rabbitMQConfig["VirtualHost"] ?? "/",
                    DispatchConsumersAsync = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare exchanges for each service
                _channel.ExchangeDeclare("product_events", ExchangeType.Topic, true);
                _channel.ExchangeDeclare("inventory_events", ExchangeType.Topic, true);
                _channel.ExchangeDeclare("order_events", ExchangeType.Topic, true);

                // Create queue for this web app
                var queueName = "inventory_management_web";
                _channel.QueueDeclare(queueName, true, false, false);

                // Bind to topics we're interested in
                _channel.QueueBind(queueName, "product_events", "product.created");
                _channel.QueueBind(queueName, "product_events", "product.updated");
                _channel.QueueBind(queueName, "inventory_events", "inventory.updated");
                _channel.QueueBind(queueName, "inventory_events", "inventory.transaction.created");
                _channel.QueueBind(queueName, "order_events", "order.created");
                _channel.QueueBind(queueName, "order_events", "order.status.changed");

                // Set up consumer
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += OnMessageReceived;

                _channel.BasicConsume(queueName, true, consumer);

                _logger.LogInformation("RabbitMQ listener started");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RabbitMQ listener");
            }
            finally
            {
                _channel?.Close();
                _connection?.Close();
            }
        }

        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;

            _logger.LogInformation("Received message with routing key {RoutingKey}: {Message}", routingKey, message);

            MessageReceived?.Invoke(routingKey, message);

            await ProcessMessage(routingKey, message);
        }

        private async Task ProcessMessage(string routingKey, string message)
        {
            try
            {
                // Process different event types
                switch (routingKey)
                {
                    case "product.created":
                        var productCreated = JsonSerializer.Deserialize<ProductCreatedEvent>(message);
                        if (productCreated != null)
                        {
                            _logger.LogInformation("Product created: {ProductId} - {Name}",
                                productCreated.ProductId, productCreated.Name);
                        }
                        break;

                    case "inventory.updated":
                        var inventoryUpdated = JsonSerializer.Deserialize<InventoryUpdatedEvent>(message);
                        if (inventoryUpdated != null)
                        {
                            _logger.LogInformation("Inventory updated: {InventoryId} - Quantity {Quantity}",
                                inventoryUpdated.InventoryId, inventoryUpdated.Quantity);
                        }
                        break;

                    case "order.created":
                        var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
                        if (orderCreated != null)
                        {
                            _logger.LogInformation("Order created: {OrderId} - Customer {CustomerName}",
                                orderCreated.OrderId, orderCreated.CustomerName);
                        }
                        break;

                    case "order.status.changed":
                        var orderStatusChanged = JsonSerializer.Deserialize<OrderStatusChangedEvent>(message);
                        if (orderStatusChanged != null)
                        {
                            _logger.LogInformation("Order status changed: {OrderId} - From {OldStatus} to {NewStatus}",
                                orderStatusChanged.OrderId, orderStatusChanged.OldStatus, orderStatusChanged.NewStatus);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RabbitMQ message");
                await Task.Delay(1);
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }

        // Event classes to deserialize messages
        private record ProductCreatedEvent
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string SKU { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int CategoryId { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private record InventoryUpdatedEvent
        {
            public int InventoryId { get; set; }
            public int ProductId { get; set; }
            public int LocationId { get; set; }
            public int Quantity { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        private record OrderCreatedEvent
        {
            public int OrderId { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public OrderStatus Status { get; set; }
            public decimal TotalAmount { get; set; }
            public DateTime OrderDate { get; set; }
        }

        private record OrderStatusChangedEvent
        {
            public int OrderId { get; set; }
            public string OldStatus { get; set; } = string.Empty;
            public string NewStatus { get; set; } = string.Empty;
            public DateTime UpdatedAt { get; set; }
        }
        private enum OrderStatus
        {
            Pending,
            Processing,
            Shipped,
            Delivered,
            Cancelled
        }
    }
}
