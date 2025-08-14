using Microsoft.EntityFrameworkCore;
using ShipmentDeliveryAPI.Models;

namespace ShipmentDeliveryAPI.Data
{
    public class ShipmentDeliveryContext : DbContext
    {
        public ShipmentDeliveryContext(DbContextOptions<ShipmentDeliveryContext> options) : base(options)
        {
        }

        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<ContainerItem> ContainerItems { get; set; }
        public DbSet<BulkItem> BulkItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Shipment entity
            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.HasKey(e => e.ShipmentId);
                entity.Property(e => e.ShipmentNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.ShipmentNumber).IsUnique();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Configure one-to-many relationship with Delivery
                entity.HasMany(s => s.Deliveries)
                      .WithOne(d => d.Shipment)
                      .HasForeignKey(d => d.ShipmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Delivery entity
            modelBuilder.Entity<Delivery>(entity =>
            {
                entity.HasKey(e => e.DeliveryId);
                entity.Property(e => e.DeliveryNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.DeliveryNumber).IsUnique();
                entity.Property(e => e.DeliveryType).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ShipmentId).IsRequired();

                // Configure one-to-many relationship with ContainerItem
                entity.HasMany(d => d.ContainerItems)
                      .WithOne(ci => ci.Delivery)
                      .HasForeignKey(ci => ci.DeliveryId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure one-to-many relationship with BulkItem
                entity.HasMany(d => d.BulkItems)
                      .WithOne(bi => bi.Delivery)
                      .HasForeignKey(bi => bi.DeliveryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ContainerItem entity
            modelBuilder.Entity<ContainerItem>(entity =>
            {
                entity.HasKey(e => e.ContainerItemId);
                entity.Property(e => e.MaterialNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.DeliveryId).IsRequired();

                // Create composite index for uniqueness
                entity.HasIndex(e => new { e.DeliveryId, e.MaterialNumber, e.SerialNumber })
                      .IsUnique()
                      .HasDatabaseName("IX_ContainerItem_Unique");
            });

            // Configure BulkItem entity
            modelBuilder.Entity<BulkItem>(entity =>
            {
                entity.HasKey(e => e.BulkItemId);
                entity.Property(e => e.MaterialNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EvdSealNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.DeliveryId).IsRequired();

                // Create composite index for uniqueness
                entity.HasIndex(e => new { e.DeliveryId, e.MaterialNumber, e.EvdSealNumber })
                      .IsUnique()
                      .HasDatabaseName("IX_BulkItem_Unique");
            });
        }
    }
}
