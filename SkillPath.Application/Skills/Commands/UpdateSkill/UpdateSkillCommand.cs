// Represents the input required to update an existing skill.
namespace SkillPath.Application.Skills.Commands.UpdateSkill;

public sealed class UpdateSkillCommand
{
    public Guid GoalId { get; init; }
    public Guid SkillId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}