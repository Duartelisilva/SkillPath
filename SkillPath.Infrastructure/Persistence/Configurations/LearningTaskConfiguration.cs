// Configures database mapping for the LearningTask entity.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillPath.Domain.Entities;

namespace SkillPath.Infrastructure.Persistence.Configurations;

public sealed class LearningTaskConfiguration : IEntityTypeConfiguration<LearningTask>
{
    public void Configure(EntityTypeBuilder<LearningTask> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.SkillId)
            .IsRequired();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.Order)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();

        builder.Property(t => t.UpdatedAtUtc);

        builder.Property(t => t.CompletedAtUtc);
    }
}