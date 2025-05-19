using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace OrderService.Infrastructure.Messaging
{
    public class RabbitMQConnection : IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMQConnection> _logger;
        private IConnection? _connection;
        private bool _disposed;

        public RabbitMQConnection(IConnectionFactory connectionFactory, ILogger<RabbitMQConnection> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        public IConnection GetConnection()
        {
            if (IsConnected)
                return _connection!;

            try
            {
                _connection = _connectionFactory.CreateConnection();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not create RabbitMQ connection: {Message}", ex.Message);
                throw;
            }

            return _connection;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disposing RabbitMQ connection: {Message}", ex.Message);
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
