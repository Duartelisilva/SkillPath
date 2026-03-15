// Defines persistence operations for goal entities.
using SkillPath.Domain.Entities;

namespace SkillPath.Application.Abstractions.Persistence;

public interface IGoalRepository
{
    Task AddAsync(Goal goal, CancellationToken cancellationToken);
    Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Goal>> ListAsync(CancellationToken cancellationToken);
    Task UpdateAsync(Goal goal, CancellationToken cancellationToken);
    Task DeleteAsync(Goal goal, CancellationToken cancellationToken);
}
