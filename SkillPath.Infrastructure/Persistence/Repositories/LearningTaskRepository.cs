// Implements learning task persistence operations using Entity Framework.
using Microsoft.EntityFrameworkCore;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Domain.Entities;

namespace SkillPath.Infrastructure.Persistence.Repositories;

public sealed class LearningTaskRepository : ILearningTaskRepository
{
    private readonly AppDbContext _dbContext;

    public LearningTaskRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(LearningTask task, CancellationToken cancellationToken)
    {
        _dbContext.Tasks.Add(task);
        return Task.CompletedTask;
    }

    public async Task<LearningTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<LearningTask>> ListBySkillAsync(Guid skillId, CancellationToken cancellationToken)
    {
        return await _dbContext.Tasks
            .Where(t => t.SkillId == skillId)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);
    }

    public Task DeleteAsync(LearningTask task, CancellationToken cancellationToken)
    {
        _dbContext.Tasks.Remove(task);
        return Task.CompletedTask;
    }
}