namespace SupplierService.Domain.Entities
{
    public class Supplier
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string ContactName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public string? Website { get; private set; }
        public string? Notes { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation property
        public ICollection<PurchaseOrder> PurchaseOrders { get; private set; } = new List<PurchaseOrder>();

        // For EF Core
        protected Supplier() { }

        public Supplier(
            string name,
            string contactName,
            string email,
            string phone,
            string address,
            string? website,
            string? notes)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Supplier name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(contactName))
                throw new ArgumentException("Contact name cannot be empty", nameof(contactName));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            Name = name;
            ContactName = contactName;
            Email = email;
            Phone = phone;
            Address = address;
            Website = website;
            Notes = notes;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            string contactName,
            string email,
            string phone,
            string address,
            string? website,
            string? notes)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Supplier name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(contactName))
                throw new ArgumentException("Contact name cannot be empty", nameof(contactName));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            Name = name;
            ContactName = contactName;
            Email = email;
            Phone = phone;
            Address = address;
            Website = website;
            Notes = notes;
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