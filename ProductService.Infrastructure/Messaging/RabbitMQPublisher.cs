using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProductService.Application.Interfaces;
using RabbitMQ.Client;

namespace ProductService.Infrastructure.Messaging
{
    public class RabbitMQPublisher : IMessagePublisher
    {
        private readonly RabbitMQConnection _connection;
        private readonly ILogger<RabbitMQPublisher> _logger;
        private readonly string _exchangeName;

        public RabbitMQPublisher(RabbitMQConnection connection, ILogger<RabbitMQPublisher> logger)
        {
            _connection = connection;
            _logger = logger;
            _exchangeName = "product_events";
        }

        public Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default) where T : class
        {
            if (!_connection.IsConnected)
            {
                _connection.GetConnection();
            }

            try
            {
                using var channel = _connection.GetConnection().CreateModel();

                channel.ExchangeDeclare(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: true);

                var body = JsonSerializer.SerializeToUtf8Bytes(message);

                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent
                properties.MessageId = Guid.NewGuid().ToString();
                properties.ContentType = "application/json";
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Published message {MessageType} with routing key {RoutingKey}",
                    typeof(T).Name, routingKey);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message {MessageType} with routing key {RoutingKey}: {Message}",
                    typeof(T).Name, routingKey, ex.Message);
                throw;
            }
        }
    }
}
