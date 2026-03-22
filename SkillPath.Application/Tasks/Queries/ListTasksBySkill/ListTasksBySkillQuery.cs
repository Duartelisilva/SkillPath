// Represents a request to list all tasks belonging to a skill.
namespace SkillPath.Application.Tasks.Queries.ListTasksBySkill;

public sealed class ListTasksBySkillQuery
{
    public Guid GoalId { get; init; }
    public Guid SkillId { get; init; }
}