// Implements skill persistence operations using Entity Framework.
using Microsoft.EntityFrameworkCore;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Domain.Entities;

namespace SkillPath.Infrastructure.Persistence.Repositories;

public sealed class SkillRepository : ISkillRepository
{
    private readonly AppDbContext _dbContext;

    public SkillRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Skill skill, CancellationToken cancellationToken)
    {
        _dbContext.Skills.Add(skill);
        return Task.CompletedTask;
    }

    public async Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Skills
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Skill>> ListByGoalAsync(Guid goalId, CancellationToken cancellationToken)
    {
        return await _dbContext.Skills
            .Include(s => s.Tasks)
            .Where(s => s.GoalId == goalId)
            .OrderBy(s => s.Order)
            .ToListAsync(cancellationToken);
    }

    public Task DeleteAsync(Skill skill, CancellationToken cancellationToken)
    {
        _dbContext.Skills.Remove(skill);
        return Task.CompletedTask;
    }
}