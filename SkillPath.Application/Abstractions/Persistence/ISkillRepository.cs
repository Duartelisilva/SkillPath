// Defines persistence operations for skill entities.
using SkillPath.Domain.Entities;

namespace SkillPath.Application.Abstractions.Persistence;

public interface ISkillRepository
{
    Task AddAsync(Skill skill, CancellationToken cancellationToken);
    Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Skill>> ListByGoalAsync(Guid goalId, CancellationToken cancellationToken);
    Task DeleteAsync(Skill skill, CancellationToken cancellationToken);
}