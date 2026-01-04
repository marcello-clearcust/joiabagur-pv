using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the PointOfSale entity.
/// </summary>
public class PointOfSaleConfiguration : IEntityTypeConfiguration<PointOfSale>
{
    public void Configure(EntityTypeBuilder<PointOfSale> builder)
    {
        builder.ToTable("PointOfSales");

        builder.HasKey(pos => pos.Id);

        builder.Property(pos => pos.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(pos => pos.Name);

        builder.Property(pos => pos.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(pos => pos.Code)
            .IsUnique();

        builder.Property(pos => pos.Address)
            .HasMaxLength(256);

        builder.Property(pos => pos.Phone)
            .HasMaxLength(20);

        builder.Property(pos => pos.Email)
            .HasMaxLength(256);

        builder.Property(pos => pos.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(pos => pos.IsActive);

        // Relationships
        builder.HasMany(pos => pos.OperatorAssignments)
            .WithOne(ups => ups.PointOfSale)
            .HasForeignKey(ups => ups.PointOfSaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
