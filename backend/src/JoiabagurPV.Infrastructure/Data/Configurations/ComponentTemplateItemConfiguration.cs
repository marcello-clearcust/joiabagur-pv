using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the ComponentTemplateItem entity.
/// </summary>
public class ComponentTemplateItemConfiguration : IEntityTypeConfiguration<ComponentTemplateItem>
{
    public void Configure(EntityTypeBuilder<ComponentTemplateItem> builder)
    {
        builder.ToTable("ComponentTemplateItems");

        builder.HasKey(i => i.Id);

        // Unique constraint: one component per template
        builder.HasIndex(i => new { i.TemplateId, i.ComponentId })
            .IsUnique();

        builder.Property(i => i.Quantity)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.HasIndex(i => i.TemplateId);
        builder.HasIndex(i => i.ComponentId);

        // Relationship with ProductComponent
        builder.HasOne(i => i.Component)
            .WithMany(c => c.TemplateItems)
            .HasForeignKey(i => i.ComponentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
