// Represents the input required to create a new goal.
namespace SkillPath.Application.Goals.Commands.CreateGoal;

public sealed class CreateGoalCommand
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
