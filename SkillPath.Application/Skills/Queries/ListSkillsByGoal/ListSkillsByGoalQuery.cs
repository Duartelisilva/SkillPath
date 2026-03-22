// Represents a request to list all skills belonging to a goal.
namespace SkillPath.Application.Skills.Queries.ListSkillsByGoal;

public sealed class ListSkillsByGoalQuery
{
    public Guid GoalId { get; init; }
}