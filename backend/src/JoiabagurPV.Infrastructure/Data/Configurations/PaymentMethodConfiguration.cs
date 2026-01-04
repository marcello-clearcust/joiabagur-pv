using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the PaymentMethod entity.
/// </summary>
public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(pm => pm.Code)
            .IsUnique();

        builder.Property(pm => pm.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(pm => pm.Name);

        builder.Property(pm => pm.Description)
            .HasMaxLength(500);

        builder.Property(pm => pm.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(pm => pm.IsActive);

        // Relationships
        builder.HasMany(pm => pm.PointOfSaleAssignments)
            .WithOne(pospm => pospm.PaymentMethod)
            .HasForeignKey(pospm => pospm.PaymentMethodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
