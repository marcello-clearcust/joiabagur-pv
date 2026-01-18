using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ReturnSale junction entity.
/// </summary>
public class ReturnSaleConfiguration : IEntityTypeConfiguration<ReturnSale>
{
    public void Configure(EntityTypeBuilder<ReturnSale> builder)
    {
        builder.ToTable("ReturnSales");

        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.ReturnId)
            .IsRequired();

        builder.Property(rs => rs.SaleId)
            .IsRequired();

        builder.Property(rs => rs.Quantity)
            .IsRequired();

        builder.Property(rs => rs.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(rs => rs.CreatedAt)
            .IsRequired();

        builder.Property(rs => rs.UpdatedAt)
            .IsRequired();

        // Unique constraint: one entry per Return-Sale combination
        builder.HasIndex(rs => new { rs.ReturnId, rs.SaleId })
            .IsUnique();

        // Index for efficient queries by SaleId
        builder.HasIndex(rs => rs.SaleId);

        // Relationships
        builder.HasOne(rs => rs.Return)
            .WithMany(r => r.ReturnSales)
            .HasForeignKey(rs => rs.ReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rs => rs.Sale)
            .WithMany(s => s.ReturnSales)
            .HasForeignKey(rs => rs.SaleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
