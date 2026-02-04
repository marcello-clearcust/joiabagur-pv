using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ReturnPhoto entity.
/// </summary>
public class ReturnPhotoConfiguration : IEntityTypeConfiguration<ReturnPhoto>
{
    public void Configure(EntityTypeBuilder<ReturnPhoto> builder)
    {
        builder.ToTable("ReturnPhotos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ReturnId)
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

        // Index for efficient queries by ReturnId
        builder.HasIndex(p => p.ReturnId)
            .IsUnique(); // One photo per return
    }
}
