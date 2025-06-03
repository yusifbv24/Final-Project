using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace InventoryService.IntegrationTests.Infrastructure
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("inventorydb_test")
            .WithUsername("test")
            .WithPassword("test123")
            .Build();

        private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .Build();

        public Mock<IProductServiceClient>? ProductServiceClientMock { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext
                services.RemoveAll(typeof(DbContextOptions<InventoryDbContext>));

                // Add PostgreSQL with test container connection string
                services.AddDbContext<InventoryDbContext>(options =>
                {
                    options.UseNpgsql(_dbContainer.GetConnectionString());
                });

                // Remove existing RabbitMQ connection
                services.RemoveAll(typeof(RabbitMQ.Client.IConnectionFactory));

                // Mock external services
                ProductServiceClientMock = new Mock<IProductServiceClient>();
                ProductServiceClientMock.Setup(x => x.ProductExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
                ProductServiceClientMock.Setup(x => x.GetProductNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((int productId, CancellationToken ct) => $"Product {productId}");

                services.RemoveAll(typeof(IProductServiceClient));
                services.AddSingleton(ProductServiceClientMock.Object);

                // Mock message publisher
                var messagePublisherMock = new Mock<IMessagePublisher>();
                services.RemoveAll(typeof(IMessagePublisher));
                services.AddSingleton(messagePublisherMock.Object);

                // Build service provider
                var sp = services.BuildServiceProvider();

                // Create and seed database
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<InventoryDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<IntegrationTestWebAppFactory>>();

                    try
                    {
                        db.Database.Migrate();
                        SeedTestData(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred during database setup.");
                        throw;
                    }
                }
            });

            builder.UseEnvironment("Testing");
        }

        public static void SeedTestData(InventoryDbContext context)
        {
            // Clear existing data
            context.InventoryTransactions.RemoveRange(context.InventoryTransactions);
            context.Inventories.RemoveRange(context.Inventories);
            context.Locations.RemoveRange(context.Locations);
            context.SaveChanges();

            // Seed locations
            var locations = new List<Domain.Entities.Location>
            {
                new("Main Warehouse", "MW001", "Main warehouse for testing"),
                new("Secondary Warehouse", "SW001", "Secondary warehouse for testing"),
                new("Store A", "ST-A", "Retail store A")
            };
            context.Locations.AddRange(locations);
            context.SaveChanges();

            // Seed inventories
            var inventories = new List<Domain.Entities.Inventory>
            {
                new(1, 1, 100), // Product 1 in Main Warehouse
                new(2, 1, 50),  // Product 2 in Main Warehouse
                new(1, 2, 75),  // Product 1 in Secondary Warehouse
                new(3, 3, 25)   // Product 3 in Store A
            };
            context.Inventories.AddRange(inventories);
            context.SaveChanges();
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            await _rabbitMqContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
            await _rabbitMqContainer.DisposeAsync();
        }
    }
}
