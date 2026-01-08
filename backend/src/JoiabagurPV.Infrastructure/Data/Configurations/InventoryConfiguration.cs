using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Inventory entity.
/// </summary>
public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventories");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.PointOfSaleId)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(i => i.LastUpdatedAt)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .IsRequired();

        // Unique constraint: one inventory record per product per POS
        builder.HasIndex(i => new { i.ProductId, i.PointOfSaleId })
            .IsUnique();

        // Index for efficient POS + Quantity queries (stock visibility)
        builder.HasIndex(i => new { i.PointOfSaleId, i.Quantity });

        // Index for efficient product lookup
        builder.HasIndex(i => i.ProductId);

        // Index for efficient POS + Product + IsActive queries
        builder.HasIndex(i => new { i.PointOfSaleId, i.ProductId, i.IsActive });

        // Relationships
        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.PointOfSale)
            .WithMany()
            .HasForeignKey(i => i.PointOfSaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Movements)
            .WithOne(m => m.Inventory)
            .HasForeignKey(m => m.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

