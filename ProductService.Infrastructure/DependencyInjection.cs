using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Interfaces;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Messaging;
using ProductService.Infrastructure.Repositories;
using RabbitMQ.Client;

namespace ProductService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ProductDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ProductDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

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
