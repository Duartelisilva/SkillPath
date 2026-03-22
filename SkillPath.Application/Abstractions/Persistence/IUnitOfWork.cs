// Abstracts the act of committing all pending changes to the database.
namespace SkillPath.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}