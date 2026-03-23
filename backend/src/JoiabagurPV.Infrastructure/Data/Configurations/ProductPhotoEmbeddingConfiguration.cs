using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the ProductPhotoEmbedding entity.
/// </summary>
public class ProductPhotoEmbeddingConfiguration : IEntityTypeConfiguration<ProductPhotoEmbedding>
{
    public void Configure(EntityTypeBuilder<ProductPhotoEmbedding> builder)
    {
        builder.ToTable("ProductPhotoEmbeddings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ProductSku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.EmbeddingVector)
            .IsRequired()
            .HasColumnType("text");

        builder.HasIndex(e => e.ProductPhotoId).IsUnique();
        builder.HasIndex(e => e.ProductId);

        builder.HasOne(e => e.ProductPhoto)
            .WithMany()
            .HasForeignKey(e => e.ProductPhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
