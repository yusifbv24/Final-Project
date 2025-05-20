using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SupplierService.Domain.Entities;

namespace SupplierService.Infrastructure.Data
{
    public class SupplierServiceSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SupplierServiceSeeder>>();

            logger.LogInformation("Seeding Supplier Service database");

            if (await context.Suppliers.AnyAsync())
            {
                logger.LogInformation("Supplier database already seeded");
                return;
            }

            // Seed Suppliers
            var suppliers = new List<Supplier>
        {
            new("ABC Electronics", "John Adams", "contact@abcelectronics.com", "555-111-2222", "123 Commerce St, Tech City, USA", "www.abcelectronics.com", "Preferred for electronics"),
            new("XYZ Furniture", "Sarah Jones", "sales@xyzfurniture.com", "555-222-3333", "456 Industry Ave, Furnish Town, USA", "www.xyzfurniture.com", "Bulk discounts available"),
            new("Fashion Forward", "Michael Lee", "orders@fashionforward.com", "555-333-4444", "789 Style Blvd, Trendy City, USA", "www.fashionforward.com", "Seasonal inventory updates"),
            new("Book Suppliers Inc.", "Emily Chen", "info@booksuppliers.com", "555-444-5555", "101 Reader Lane, Bookville, USA", "www.booksuppliers.com", "Educational discounts available"),
            new("Toy Universe", "David Park", "sales@toyuniverse.com", "555-555-6666", "202 Play Street, Funtown, USA", "www.toyuniverse.com", "Specializes in educational toys"),
            new("Sports Emporium", "Jessica Kim", "orders@sportsemporium.com", "555-666-7777", "303 Athletic Drive, Sportsville, USA", "www.sportsemporium.com", "Quality sporting goods"),
            new("Kitchen Essentials", "Robert Singh", "info@kitchenessentials.com", "555-777-8888", "404 Culinary Court, Cookingtown, USA", "www.kitchenessentials.com", "Wholesale kitchen supplies"),
            new("Beauty Basics", "Nicole Taylor", "sales@beautybasics.com", "555-888-9999", "505 Glamour Road, Beautyville, USA", "www.beautybasics.com", "Organic and cruelty-free products"),
            new("Auto Parts Direct", "William Johnson", "parts@autopartsdirect.com", "555-999-0000", "606 Motor Street, Cartown, USA", "www.autopartsdirect.com", "Fast shipping on all parts"),
            new("Green Thumb Gardens", "Emma Garcia", "info@greenthumbgardens.com", "555-000-1111", "707 Plant Drive, Gardenville, USA", "www.greenthumbgardens.com", "Seasonal plants and tools")
        };

            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();

            // Seed Purchase Orders
            var purchaseOrders = new List<PurchaseOrder>();

            for (int i = 1; i <= 10; i++)
            {
                var supplierId = (i % 10) + 1;
                var orderNumber = $"PO-{2000 + i}";
                var expectedDelivery = DateTime.UtcNow.AddDays(14 + i);

                var po = new PurchaseOrder(
                    supplierId,
                    orderNumber,
                    expectedDelivery,
                    i % 3 == 0 ? "High priority" : null
                );

                if (i % 3 == 0)
                    po.UpdateStatus(PurchaseOrderStatus.Submitted);
                else if (i % 5 == 0)
                    po.UpdateStatus(PurchaseOrderStatus.Approved);

                purchaseOrders.Add(po);
            }

            await context.PurchaseOrders.AddRangeAsync(purchaseOrders);
            await context.SaveChangesAsync();

            // Seed Purchase Order Items (assuming ProductIds 1-30 exist in ProductService)
            var poItems = new List<PurchaseOrderItem>();

            foreach (var po in purchaseOrders)
            {
                // Add 2-3 items to each PO
                var itemCount = Random.Shared.Next(2, 4);

                for (int i = 0; i < itemCount; i++)
                {
                    var productId = Random.Shared.Next(1, 30);
                    var productName = $"Product {productId}";
                    var quantity = Random.Shared.Next(10, 100);
                    var unitPrice = (decimal)(Random.Shared.NextDouble() * 100 + 5); // Random price $5-$105

                    poItems.Add(new PurchaseOrderItem(po.Id, productId, productName, quantity, unitPrice));
                }
            }

            await context.PurchaseOrderItems.AddRangeAsync(poItems);
            await context.SaveChangesAsync();

            logger.LogInformation("Supplier database seeded successfully");
        }
    }
}
