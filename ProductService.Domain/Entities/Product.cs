namespace ProductService.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string SKU { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int CategoryId { get; private set; }
        public Category? Category { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // For EF Core
        protected Product() { }

        public Product(string name, string description, string sku, decimal price, int categoryId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty", nameof(name));

            if (price < 0)
                throw new ArgumentException("Product price cannot be negative", nameof(price));

            Name = name;
            Description = description;
            SKU = sku;
            Price = price;
            CategoryId = categoryId;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string description, string sku, decimal price, int categoryId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty", nameof(name));

            if (price < 0)
                throw new ArgumentException("Product price cannot be negative", nameof(price));

            Name = name;
            Description = description;
            SKU = sku;
            Price = price;
            CategoryId = categoryId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
