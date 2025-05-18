namespace InventoryService.Domain.Entities
{
    public class Location
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Code { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation property
        public ICollection<Inventory> Inventories { get; private set; } = new List<Inventory>();

        // For EF Core
        protected Location() { }

        public Location(string name, string code, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Location name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Location code cannot be empty", nameof(code));

            Name = name;
            Code = code;
            Description = description;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string code, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Location name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Location code cannot be empty", nameof(code));

            Name = name;
            Code = code;
            Description = description;
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