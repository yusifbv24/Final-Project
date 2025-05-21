using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.ExternalServices;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Repositories;
using RabbitMQ.Client;

namespace OrderService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<OrderDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // External Services
            services.AddHttpClient<IProductServiceClient, ProductServiceClient>();
            services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>();

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
