using InventoryService.Application.Interfaces;
using InventoryService.Domain.Repositories;
using InventoryService.Infrastructure.Data;
using InventoryService.Infrastructure.ExternalServices;
using InventoryService.Infrastructure.Messaging;
using InventoryService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace InventoryService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // External Services
            services.AddHttpClient<IProductServiceClient, ProductServiceClient>();

            // RabbitMQ
            services.AddSingleton<IConnectionFactory>(sp =>
            {
                var rabbitMQConfig = configuration.GetSection("RabbitMQ");

                return new ConnectionFactory
                {
                    HostName = rabbitMQConfig["Host"] ?? "localhost",
                    Port = int.Parse(rabbitMQConfig["Port"] ?? "5672"),
                    UserName = rabbitMQConfig["Username"] ?? "guest",
                    Password = rabbitMQConfig["Password"] ?? "guest",
                    VirtualHost = rabbitMQConfig["VirtualHost"] ?? "/",
                    DispatchConsumersAsync = true
                };
            });

            services.AddSingleton<RabbitMQConnection>();
            services.AddScoped<IMessagePublisher, RabbitMQPublisher>();

            return services;
        }
    }
}
