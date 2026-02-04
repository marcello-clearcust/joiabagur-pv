using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Return entity.
/// </summary>
public class ReturnConfiguration : IEntityTypeConfiguration<Return>
{
    public void Configure(EntityTypeBuilder<Return> builder)
    {
        builder.ToTable("Returns");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ProductId)
            .IsRequired();

        builder.Property(r => r.PointOfSaleId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.Quantity)
            .IsRequired();

        builder.Property(r => r.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.Reason)
            .HasMaxLength(500);

        builder.Property(r => r.ReturnDate)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        // Indexes as defined in data model
        builder.HasIndex(r => new { r.PointOfSaleId, r.ReturnDate });
        builder.HasIndex(r => new { r.ProductId, r.ReturnDate });

        // Relationships
        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.PointOfSale)
            .WithMany()
            .HasForeignKey(r => r.PointOfSaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Photo)
            .WithOne(p => p.Return)
            .HasForeignKey<ReturnPhoto>(p => p.ReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.InventoryMovement)
            .WithOne()
            .HasForeignKey<InventoryMovement>(m => m.ReturnId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
