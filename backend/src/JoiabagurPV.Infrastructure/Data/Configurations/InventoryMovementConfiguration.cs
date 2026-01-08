using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for InventoryMovement entity.
/// </summary>
public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("InventoryMovements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.InventoryId)
            .IsRequired();

        builder.Property(m => m.SaleId);

        builder.Property(m => m.ReturnId);

        builder.Property(m => m.UserId)
            .IsRequired();

        builder.Property(m => m.MovementType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.QuantityChange)
            .IsRequired();

        builder.Property(m => m.QuantityBefore)
            .IsRequired();

        builder.Property(m => m.QuantityAfter)
            .IsRequired();

        builder.Property(m => m.Reason)
            .HasMaxLength(500);

        builder.Property(m => m.MovementDate)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

        // Index for efficient inventory movement queries
        builder.HasIndex(m => m.InventoryId);

        // Index for date-based queries
        builder.HasIndex(m => m.MovementDate);

        // Composite index for filtered queries
        builder.HasIndex(m => new { m.InventoryId, m.MovementDate });

        // Relationships
        builder.HasOne(m => m.Inventory)
            .WithMany(i => i.Movements)
            .HasForeignKey(m => m.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

