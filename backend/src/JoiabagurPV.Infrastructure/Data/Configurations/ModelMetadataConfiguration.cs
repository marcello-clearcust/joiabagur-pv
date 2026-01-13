using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ModelMetadata entity.
/// </summary>
public class ModelMetadataConfiguration : IEntityTypeConfiguration<ModelMetadata>
{
    public void Configure(EntityTypeBuilder<ModelMetadata> builder)
    {
        builder.ToTable("ModelMetadata");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Version)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(m => m.Version)
            .IsUnique();

        builder.Property(m => m.TrainedAt)
            .IsRequired();

        builder.Property(m => m.ModelPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.AccuracyMetrics)
            .HasMaxLength(2000);

        builder.Property(m => m.TotalPhotosUsed)
            .IsRequired();

        builder.Property(m => m.TotalProductsUsed)
            .IsRequired();

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(m => m.IsActive);

        builder.Property(m => m.Notes)
            .HasMaxLength(1000);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .IsRequired();
    }
}
