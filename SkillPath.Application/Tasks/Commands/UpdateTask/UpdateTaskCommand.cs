// Represents the input required to update an existing task.
namespace SkillPath.Application.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommand
{
    public Guid GoalId { get; init; }
    public Guid SkillId { get; init; }
    public Guid TaskId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}