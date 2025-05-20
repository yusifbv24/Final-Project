using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Data
{
    public class ProductServiceSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ProductServiceSeeder>>();

            logger.LogInformation("Seeding Product Service database");

            if (await context.Categories.AnyAsync())
            {
                logger.LogInformation("Product database already seeded");
                return;
            }

            // Seed Categories
            var categories = new List<Category>
        {
            new("Electronics", "Electronic devices and accessories"),
            new("Furniture", "Home and office furniture"),
            new("Clothing", "Men's, women's, and children's clothing"),
            new("Books", "Books, e-books, and audiobooks"),
            new("Toys", "Toys for children of all ages"),
            new("Sports", "Sports equipment and accessories"),
            new("Kitchen", "Kitchen appliances and utensils"),
            new("Beauty", "Beauty and personal care products"),
            new("Automotive", "Automotive parts and accessories"),
            new("Garden", "Garden tools and outdoor equipment")
        };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // Seed Products (10 per category)
            var products = new List<Product>
            {
                // Electronics
                new("Smartphone X", "Latest smartphone with 6.5-inch display", "SM-X-001", 899.99m, 1),
                new("Laptop Pro", "Professional laptop with 16GB RAM", "LP-001", 1299.99m, 1),
                new("Wireless Earbuds", "Noise-cancelling wireless earbuds", "WE-001", 129.99m, 1),
                new("4K Smart TV", "55-inch 4K Smart TV", "TV-001", 799.99m, 1),
                new("Digital Camera", "20MP digital camera with zoom lens", "DC-001", 349.99m, 1),
                new("Gaming Console", "Next-gen gaming console", "GC-001", 499.99m, 1),
                new("Bluetooth Speaker", "Portable Bluetooth speaker", "BS-001", 79.99m, 1),
                new("Smartwatch", "Fitness tracking smartwatch", "SW-001", 179.99m, 1),
                new("Tablet", "10-inch tablet with 64GB storage", "TB-001", 299.99m, 1),
                new("Wireless Mouse", "Ergonomic wireless mouse", "WM-001", 24.99m, 1),

                // Furniture
                new("Office Chair", "Ergonomic office chair", "OC-001", 149.99m, 2),
                new("Standing Desk", "Adjustable standing desk", "SD-001", 249.99m, 2),
                new("Bookshelf", "5-tier bookshelf", "BS-002", 89.99m, 2),
                new("Sofa", "3-seater sofa", "SF-001", 499.99m, 2),
                new("Coffee Table", "Wooden coffee table", "CT-001", 129.99m, 2),
                new("Bed Frame", "Queen size bed frame", "BF-001", 299.99m, 2),
                new("Dining Table", "6-person dining table", "DT-001", 399.99m, 2),
                new("Wardrobe", "2-door wardrobe", "WD-001", 249.99m, 2),
                new("TV Stand", "Modern TV stand", "TVS-001", 99.99m, 2),
                new("Desk Lamp", "LED desk lamp", "DL-001", 39.99m, 2),

                // Add more products for other categories...
                // Clothing
                new("Men's T-Shirt", "Cotton crew neck t-shirt", "MT-001", 19.99m, 3),
                new("Women's Dress", "Summer dress", "WD-001", 49.99m, 3),
                new("Jeans", "Classic blue jeans", "JN-001", 39.99m, 3),
                new("Hoodie", "Pullover hoodie", "HD-001", 29.99m, 3),
                new("Sneakers", "Running sneakers", "SN-001", 69.99m, 3),
                new("Socks", "Pack of 6 cotton socks", "SK-001", 12.99m, 3),
                new("Winter Jacket", "Insulated winter jacket", "WJ-001", 89.99m, 3),
                new("Hat", "Knit beanie hat", "HT-001", 14.99m, 3),
                new("Gloves", "Touchscreen compatible gloves", "GL-001", 19.99m, 3),
                new("Scarf", "Warm winter scarf", "SC-001", 17.99m, 3)
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            logger.LogInformation("Product database seeded successfully");
        }
    }
}
