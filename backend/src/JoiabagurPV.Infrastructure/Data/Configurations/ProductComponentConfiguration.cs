using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the ProductComponent entity (master table).
/// </summary>
public class ProductComponentConfiguration : IEntityTypeConfiguration<ProductComponent>
{
    public void Configure(EntityTypeBuilder<ProductComponent> builder)
    {
        builder.ToTable("ProductComponents");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(35);

        builder.HasIndex(c => c.Description)
            .IsUnique();

        builder.Property(c => c.CostPrice)
            .HasPrecision(18, 4);

        builder.Property(c => c.SalePrice)
            .HasPrecision(18, 4);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(c => c.IsActive);

        // Relationships configured on the dependent side (Assignment, TemplateItem)
    }
}
