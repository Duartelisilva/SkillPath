// Defines persistence operations for learning task entities.
using SkillPath.Domain.Entities;

namespace SkillPath.Application.Abstractions.Persistence;

public interface ILearningTaskRepository
{
    Task AddAsync(LearningTask task, CancellationToken cancellationToken);
    Task<LearningTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LearningTask>> ListBySkillAsync(Guid skillId, CancellationToken cancellationToken);
    Task DeleteAsync(LearningTask task, CancellationToken cancellationToken);
}