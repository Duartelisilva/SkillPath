// Represents the base entity for domain models, providing a common identifier.
namespace SkillPath.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
