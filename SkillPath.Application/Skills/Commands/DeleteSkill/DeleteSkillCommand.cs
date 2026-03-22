// Represents the input required to delete a skill.
namespace SkillPath.Application.Skills.Commands.DeleteSkill;

public sealed class DeleteSkillCommand
{
    public Guid GoalId { get; init; }
    public Guid SkillId { get; init; }
}