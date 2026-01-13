using JoiabagurPV.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoiabagurPV.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ModelTrainingJob entity.
/// </summary>
public class ModelTrainingJobConfiguration : IEntityTypeConfiguration<ModelTrainingJob>
{
    public void Configure(EntityTypeBuilder<ModelTrainingJob> builder)
    {
        builder.ToTable("ModelTrainingJobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.InitiatedBy)
            .IsRequired();

        builder.Property(j => j.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(j => j.Status);

        builder.Property(j => j.ProgressPercentage)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(j => j.CurrentStage)
            .HasMaxLength(500);

        builder.Property(j => j.StartedAt);

        builder.Property(j => j.CompletedAt);

        builder.Property(j => j.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(j => j.ResultModelVersion)
            .HasMaxLength(50);

        builder.Property(j => j.DurationSeconds);

        builder.Property(j => j.CreatedAt)
            .IsRequired();

        builder.Property(j => j.UpdatedAt)
            .IsRequired();

        builder.HasIndex(j => j.CreatedAt);
        builder.HasIndex(j => new { j.Status, j.CreatedAt });

        // Relationship with User
        builder.HasOne(j => j.InitiatedByUser)
            .WithMany()
            .HasForeignKey(j => j.InitiatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
