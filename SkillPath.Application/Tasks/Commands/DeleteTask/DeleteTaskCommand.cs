// Represents the input required to delete a task.
namespace SkillPath.Application.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskCommand
{
    public Guid GoalId { get; init; }
    public Guid SkillId { get; init; }
    public Guid TaskId { get; init; }
}