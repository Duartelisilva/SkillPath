// Represents the Entity Framework database context for the application.
using Microsoft.EntityFrameworkCore;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Domain.Entities;

namespace SkillPath.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<LearningTask> Tasks => Set<LearningTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}