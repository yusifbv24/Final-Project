using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using SupplierService.Application.Interfaces;
using SupplierService.Domain.Repositories;
using SupplierService.Infrastructure.Data;
using SupplierService.Infrastructure.ExternalServices;
using SupplierService.Infrastructure.Messaging;
using SupplierService.Infrastructure.Repositories;

namespace SupplierService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<SupplierDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(SupplierDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            services.AddScoped<IPurchaseOrderItemRepository, PurchaseOrderItemRepository>();
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
