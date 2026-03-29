// Represents the input required to update a task's status.
namespace SkillPath.Application.Tasks.Commands.UpdateTaskStatus;

public sealed class UpdateTaskStatusCommand
{
    public Guid GoalId { get; init; }
    public Guid SkillId { get; init; }
    public Guid TaskId { get; init; }
    public string Status { get; init; } = string.Empty; // "NotStarted", "InProgress", "Completed"
}