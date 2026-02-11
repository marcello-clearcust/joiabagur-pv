using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the ComponentTemplate entity.
/// </summary>
public class ComponentTemplateConfiguration : IEntityTypeConfiguration<ComponentTemplate>
{
    public void Configure(EntityTypeBuilder<ComponentTemplate> builder)
    {
        builder.ToTable("ComponentTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Name);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        // Relationship with TemplateItems
        builder.HasMany(t => t.Items)
            .WithOne(i => i.Template)
            .HasForeignKey(i => i.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
