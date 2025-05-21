using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data
{
    public class OrderServiceSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderServiceSeeder>>();

            logger.LogInformation("Seeding Order Service database");

            if (await context.Orders.AnyAsync())
            {
                logger.LogInformation("Order database already seeded");
                return;
            }

            // Sample orders
            var orders = new List<Order>
            {
                new("John Doe", "john@example.com", "123 Main St, New York, NY 10001"),
                new("Jane Smith", "jane@example.com", "456 Elm St, Los Angeles, CA 90001"),
                new("Michael Johnson", "michael@example.com", "789 Oak St, Chicago, IL 60007")
            };

            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();

            // Sample order items (using product IDs 1-10 from ProductService)
            var orderItems = new List<OrderItem>
            {
                new(1, 1, "Smartphone X", 899.99m, 1),
                new(1, 3, "Wireless Earbuds", 129.99m, 2),
                new(2, 5, "Digital Camera", 349.99m, 1),
                new(2, 8, "Smartwatch", 179.99m, 1),
                new(3, 2, "Laptop Pro", 1299.99m, 1),
                new(3, 7, "Bluetooth Speaker", 79.99m, 3)
            };

            await context.OrderItems.AddRangeAsync(orderItems);
            await context.SaveChangesAsync();

            // Update order statuses and total amounts
            var order1 = await context.Orders.FindAsync(1);
            var order2 = await context.Orders.FindAsync(2);
            var order3 = await context.Orders.FindAsync(3);

            if (order1 != null)
            {
                order1.UpdateStatus(OrderStatus.Delivered);
            }

            if (order2 != null)
            {
                order2.UpdateStatus(OrderStatus.Shipped);
            }

            if (order3 != null)
            {
                order3.UpdateStatus(OrderStatus.Processing);
            }

            await context.SaveChangesAsync();

            logger.LogInformation("Order database seeded successfully");
        }
    }
}
