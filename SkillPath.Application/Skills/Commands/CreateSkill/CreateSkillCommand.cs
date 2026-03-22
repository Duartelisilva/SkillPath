// Represents the input required to create a new skill.
namespace SkillPath.Application.Skills.Commands.CreateSkill;

public sealed class CreateSkillCommand
{
    public Guid GoalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Order { get; init; }
}