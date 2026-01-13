using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for SalePhoto entity.
/// </summary>
public class SalePhotoConfiguration : IEntityTypeConfiguration<SalePhoto>
{
    public void Configure(EntityTypeBuilder<SalePhoto> builder)
    {
        builder.ToTable("SalePhotos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.SaleId)
            .IsRequired();

        builder.Property(p => p.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.FileSize)
            .IsRequired();

        builder.Property(p => p.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Index for querying photos by sale
        builder.HasIndex(p => p.SaleId)
            .IsUnique();

        // Relationship configured in SaleConfiguration
    }
}
