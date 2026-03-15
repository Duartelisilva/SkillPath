// Represents the input required to retrieve a goal by identifier.
namespace SkillPath.Application.Goals.Queries.GetGoalById;

public sealed class GetGoalByIdQuery
{
    public Guid Id { get; init; }
}
