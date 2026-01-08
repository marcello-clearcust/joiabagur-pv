using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the ProductPhoto entity.
/// </summary>
public class ProductPhotoConfiguration : IEntityTypeConfiguration<ProductPhoto>
{
    public void Configure(EntityTypeBuilder<ProductPhoto> builder)
    {
        builder.ToTable("ProductPhotos");

        builder.HasKey(pp => pp.Id);

        builder.Property(pp => pp.FileName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pp => pp.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(pp => pp.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(pp => pp.ProductId);

        builder.HasIndex(pp => new { pp.ProductId, pp.DisplayOrder });
    }
}




