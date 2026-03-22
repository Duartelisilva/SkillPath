// Represents a request to retrieve a single skill by identifier.
namespace SkillPath.Application.Skills.Queries.GetSkillById;

public sealed class GetSkillByIdQuery
{
    public Guid GoalId { get; init; }
    public Guid SkillId { get; init; }
}