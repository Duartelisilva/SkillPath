// Implements goal persistence operations using Entity Framework.
using Microsoft.EntityFrameworkCore;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Domain.Entities;

namespace SkillPath.Infrastructure.Persistence.Repositories;

public sealed class GoalRepository : IGoalRepository
{
    private readonly AppDbContext _dbContext;

    public GoalRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Goal goal, CancellationToken cancellationToken)
    {
        _dbContext.Goals.Add(goal);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Goal goal, CancellationToken cancellationToken)
    {
        _dbContext.Goals.Update(goal);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Goal goal, CancellationToken cancellationToken)
    {
        _dbContext.Goals.Remove(goal);
        return Task.CompletedTask;
    }

    public async Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Goals
            .FirstOrDefaultAsync(goal => goal.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Goal>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Goals
            .OrderBy(goal => goal.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
