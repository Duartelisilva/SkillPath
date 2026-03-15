// Configures database mapping for the Goal entity.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillPath.Domain.Entities;

namespace SkillPath.Infrastructure.Persistence.Configurations;

public sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("Goals");

        builder.HasKey(goal => goal.Id);

        builder.Property(goal => goal.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(goal => goal.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(goal => goal.Status)
            .IsRequired();

        builder.Property(goal => goal.CreatedAtUtc)
            .IsRequired();

        builder.Property(goal => goal.UpdatedAtUtc);
    }
}
