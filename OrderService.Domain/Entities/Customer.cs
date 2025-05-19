namespace OrderService.Domain.Entities
{
    public class Customer
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public string DefaultShippingAddress { get; private set; } = string.Empty;
        public string DefaultBillingAddress { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation property
        public ICollection<Order> Orders { get; private set; } = new List<Order>();

        // For EF Core
        protected Customer() { }

        public Customer(
            string name,
            string email,
            string phone,
            string defaultShippingAddress,
            string defaultBillingAddress)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Customer name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Customer email cannot be empty", nameof(email));

            Name = name;
            Email = email;
            Phone = phone;
            DefaultShippingAddress = defaultShippingAddress;
            DefaultBillingAddress = defaultBillingAddress;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            string email,
            string phone,
            string defaultShippingAddress,
            string defaultBillingAddress)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Customer name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Customer email cannot be empty", nameof(email));

            Name = name;
            Email = email;
            Phone = phone;
            DefaultShippingAddress = defaultShippingAddress;
            DefaultBillingAddress = defaultBillingAddress;
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