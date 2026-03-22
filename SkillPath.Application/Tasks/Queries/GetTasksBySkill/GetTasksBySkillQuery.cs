// Represents a request to retrieve all tasks for a skill.
namespace SkillPath.Application.Tasks.Queries.GetTasksBySkill;

public sealed class GetTasksBySkillQuery
{
    public Guid GoalId { get; init; }
    public Guid SkillId { get; init; }
}