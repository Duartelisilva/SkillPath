// Represents the input required to update an existing goal.
namespace SkillPath.Application.Goals.Commands.UpdateGoal;

public sealed class UpdateGoalCommand
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
