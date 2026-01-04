using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the UserPointOfSale entity.
/// </summary>
public class UserPointOfSaleConfiguration : IEntityTypeConfiguration<UserPointOfSale>
{
    public void Configure(EntityTypeBuilder<UserPointOfSale> builder)
    {
        builder.ToTable("UserPointOfSales");

        builder.HasKey(ups => ups.Id);

        builder.Property(ups => ups.UserId)
            .IsRequired();

        builder.Property(ups => ups.PointOfSaleId)
            .IsRequired();

        builder.Property(ups => ups.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ups => ups.AssignedAt)
            .IsRequired();

        builder.Property(ups => ups.UnassignedAt);

        // Composite unique index for active assignments
        // A user can only have one active assignment per point of sale
        builder.HasIndex(ups => new { ups.UserId, ups.PointOfSaleId, ups.IsActive })
            .HasFilter("\"IsActive\" = true")
            .IsUnique();

        // Index for querying user assignments
        builder.HasIndex(ups => ups.UserId);

        // Index for querying point of sale users
        builder.HasIndex(ups => ups.PointOfSaleId);
    }
}
