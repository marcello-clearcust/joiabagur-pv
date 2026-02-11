using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the ProductComponentAssignment entity.
/// </summary>
public class ProductComponentAssignmentConfiguration : IEntityTypeConfiguration<ProductComponentAssignment>
{
    public void Configure(EntityTypeBuilder<ProductComponentAssignment> builder)
    {
        builder.ToTable("ProductComponentAssignments");

        builder.HasKey(a => a.Id);

        // Unique constraint: one component per product
        builder.HasIndex(a => new { a.ProductId, a.ComponentId })
            .IsUnique();

        builder.Property(a => a.Quantity)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(a => a.CostPrice)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(a => a.SalePrice)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(a => a.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(a => a.ProductId);
        builder.HasIndex(a => a.ComponentId);

        // Relationship with Product
        builder.HasOne(a => a.Product)
            .WithMany()
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with ProductComponent
        builder.HasOne(a => a.Component)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.ComponentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
