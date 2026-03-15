// Represents the input required to delete a goal.
namespace SkillPath.Application.Goals.Commands.DeleteGoal;

public sealed class DeleteGoalCommand
{
    public Guid Id { get; init; }
}
