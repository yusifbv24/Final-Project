using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Data
{
    public class InventoryDbContext : DbContext
    {
        public DbSet<Inventory> Inventories { get; set; } = null!;
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;

        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Location Configuration
            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Locations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Inventory Configuration
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("Inventories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.LocationId).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Create a unique constraint on ProductId and LocationId
                entity.HasIndex(e => new { e.ProductId, e.LocationId }).IsUnique();

                // Relationships
                entity.HasOne(e => e.Location)
                    .WithMany(l => l.Inventories)
                    .HasForeignKey(e => e.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // InventoryTransaction Configuration
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.ToTable("InventoryTransactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InventoryId).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.Reference).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.TransactionDate).IsRequired();

                // Relationships
                entity.HasOne(e => e.Inventory)
                    .WithMany(i => i.Transactions)
                    .HasForeignKey(e => e.InventoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
