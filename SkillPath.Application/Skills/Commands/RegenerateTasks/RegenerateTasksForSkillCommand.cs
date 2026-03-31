// Represents the input required to regenerate tasks for a skill.
namespace SkillPath.Application.Skills.Commands.RegenerateTasks;

public sealed class RegenerateTasksForSkillCommand
{
    public Guid GoalId { get; init; }
    public Guid SkillId { get; init; }
}