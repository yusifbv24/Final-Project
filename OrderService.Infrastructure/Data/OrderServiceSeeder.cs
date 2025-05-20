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

            if (await context.Customers.AnyAsync())
            {
                logger.LogInformation("Order database already seeded");
                return;
            }

            // Seed Customers
            var customers = new List<Customer>
        {
            new("John Smith", "john.smith@example.com", "555-123-4567", "123 Main St, Anytown, USA", "123 Main St, Anytown, USA"),
            new("Jane Doe", "jane.doe@example.com", "555-234-5678", "456 Elm St, Somewhere, USA", "456 Elm St, Somewhere, USA"),
            new("Mike Johnson", "mike.johnson@example.com", "555-345-6789", "789 Oak St, Nowhere, USA", "789 Oak St, Nowhere, USA"),
            new("Sarah Williams", "sarah.williams@example.com", "555-456-7890", "321 Pine St, Elsewhere, USA", "321 Pine St, Elsewhere, USA"),
            new("David Brown", "david.brown@example.com", "555-567-8901", "654 Maple St, Anywhere, USA", "654 Maple St, Anywhere, USA"),
            new("Emily Davis", "emily.davis@example.com", "555-678-9012", "987 Cedar St, Someplace, USA", "987 Cedar St, Someplace, USA"),
            new("Robert Wilson", "robert.wilson@example.com", "555-789-0123", "159 Birch St, Othertown, USA", "159 Birch St, Othertown, USA"),
            new("Jennifer Taylor", "jennifer.taylor@example.com", "555-890-1234", "753 Walnut St, Thisplace, USA", "753 Walnut St, Thisplace, USA"),
            new("Michael Anderson", "michael.anderson@example.com", "555-901-2345", "852 Cherry St, Thatplace, USA", "852 Cherry St, Thatplace, USA"),
            new("Lisa Thomas", "lisa.thomas@example.com", "555-012-3456", "963 Spruce St, Yourtown, USA", "963 Spruce St, Yourtown, USA")
        };

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();

            // Seed Orders
            var orders = new List<Order>();

            for (int i = 1; i <= 10; i++)
            {
                var customerId = (i % 10) + 1;
                var customer = await context.Customers.FindAsync(customerId);

                var order = new Order(
                    customerId,
                    customer!.DefaultShippingAddress,
                    customer.DefaultBillingAddress,
                    i % 3 == 0 ? "Priority shipping" : null
                );

                if (i % 3 == 0)
                    order.UpdateStatus(OrderStatus.Processing);
                else if (i % 5 == 0)
                    order.UpdateStatus(OrderStatus.Shipped);

                orders.Add(order);
            }

            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();

            // Seed Order Items (assuming ProductIds 1-30 exist in ProductService)
            var orderItems = new List<OrderItem>();

            foreach (var order in orders)
            {
                // Add 2-3 items to each order
                var itemCount = Random.Shared.Next(2, 4);

                for (int i = 0; i < itemCount; i++)
                {
                    var productId = Random.Shared.Next(1, 30);
                    var productName = $"Product {productId}";
                    var quantity = Random.Shared.Next(1, 5);
                    var unitPrice = (decimal)(Random.Shared.NextDouble() * 100 + 10); // Random price $10-$110

                    orderItems.Add(new OrderItem(order.Id, productId, productName, quantity, unitPrice));
                }
            }

            await context.OrderItems.AddRangeAsync(orderItems);
            await context.SaveChangesAsync();

            logger.LogInformation("Order database seeded successfully");
        }
    }
}
