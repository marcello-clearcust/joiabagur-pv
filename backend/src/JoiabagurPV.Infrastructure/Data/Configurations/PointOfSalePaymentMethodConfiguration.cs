using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the PointOfSalePaymentMethod entity.
/// </summary>
public class PointOfSalePaymentMethodConfiguration : IEntityTypeConfiguration<PointOfSalePaymentMethod>
{
    public void Configure(EntityTypeBuilder<PointOfSalePaymentMethod> builder)
    {
        builder.ToTable("PointOfSalePaymentMethods");

        builder.HasKey(pospm => pospm.Id);

        // Composite unique constraint
        builder.HasIndex(pospm => new { pospm.PointOfSaleId, pospm.PaymentMethodId })
            .IsUnique();

        builder.Property(pospm => pospm.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(pospm => pospm.IsActive);

        builder.Property(pospm => pospm.DeactivatedAt);

        // Relationships
        builder.HasOne(pospm => pospm.PointOfSale)
            .WithMany(pos => pos.PaymentMethodAssignments)
            .HasForeignKey(pospm => pospm.PointOfSaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pospm => pospm.PaymentMethod)
            .WithMany(pm => pm.PointOfSaleAssignments)
            .HasForeignKey(pospm => pospm.PaymentMethodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
