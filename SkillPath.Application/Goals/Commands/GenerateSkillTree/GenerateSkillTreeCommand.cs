// Represents the input required to generate a skill tree for a goal.
namespace SkillPath.Application.Goals.Commands.GenerateSkillTree;

public sealed class GenerateSkillTreeCommand
{
    public Guid GoalId { get; init; }
    public string? AdditionalContext { get; init; }
}