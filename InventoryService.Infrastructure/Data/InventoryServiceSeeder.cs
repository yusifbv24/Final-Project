using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InventoryService.Infrastructure.Data
{
    public class InventoryServiceSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<InventoryServiceSeeder>>();

            logger.LogInformation("Seeding Inventory Service database");

            if (await context.Locations.AnyAsync())
            {
                logger.LogInformation("Inventory database already seeded");
                return;
            }

            // Seed Locations
            var locations = new List<Location>
        {
            new("Main Warehouse", "MW", "Central warehouse for all products"),
            new("North Warehouse", "NW", "North regional warehouse"),
            new("South Warehouse", "SW", "South regional warehouse"),
            new("East Warehouse", "EW", "East regional warehouse"),
            new("West Warehouse", "WW", "West regional warehouse"),
            new("Store A", "ST-A", "Retail store location A"),
            new("Store B", "ST-B", "Retail store location B"),
            new("Store C", "ST-C", "Retail store location C"),
            new("Distribution Center", "DC", "Main distribution center"),
            new("Returns Department", "RD", "Returns processing department")
        };

            await context.Locations.AddRangeAsync(locations);
            await context.SaveChangesAsync();

            // Seed Inventories (assuming ProductIds 1-30 exist in ProductService)
            var inventories = new List<Inventory>();

            // Main Warehouse (LocationId = 1)
            for (int i = 1; i <= 10; i++)
            {
                inventories.Add(new Inventory(i, 1, Random.Shared.Next(50, 200)));
            }

            // North Warehouse (LocationId = 2)
            for (int i = 11; i <= 20; i++)
            {
                inventories.Add(new Inventory(i, 2, Random.Shared.Next(20, 100)));
            }

            // Store A (LocationId = 6)
            for (int i = 1; i <= 10; i++)
            {
                inventories.Add(new Inventory(i, 6, Random.Shared.Next(5, 30)));
            }

            await context.Inventories.AddRangeAsync(inventories);
            await context.SaveChangesAsync();

            // Seed Inventory Transactions
            var transactions = new List<InventoryTransaction>
            {
                new (1, TransactionType.StockIn, 50, "Initial Stock", "Initial inventory load"),
                new (1, TransactionType.StockOut, 10, "Order-1001", "For customer order"),
                new (2, TransactionType.StockIn, 25, "PO-2001", "From supplier ABC Electronics"),
                new (3, TransactionType.StockIn, 100, "Initial Stock", "Initial inventory load"),
                new (3, TransactionType.StockOut, 15, "Order-1002", "For customer order"),
                new (4, TransactionType.StockIn, 20, "PO-2002", "From supplier XYZ Electronics"),
                new (5, TransactionType.Adjustment, 5, "Inventory Count", "Adjustment after physical count"),
                new (6, TransactionType.StockIn, 20, "PO-2003", "From supplier ABC Electronics"),
                new (7, TransactionType.Transfer, 10, "Transfer-1001", "Transfer to Store A"),
                new (8, TransactionType.StockOut, 5, "Order-1003", "For customer order"),
                new (9, TransactionType.StockIn, 30, "PO-2004", "From supplier XYZ Electronics"),
                new (10, TransactionType.Adjustment, -3, "Damage", "Items damaged during handling")
            };

            await context.InventoryTransactions.AddRangeAsync(transactions);
            await context.SaveChangesAsync();

            logger.LogInformation("Inventory database seeded successfully");
        }
    }
}
