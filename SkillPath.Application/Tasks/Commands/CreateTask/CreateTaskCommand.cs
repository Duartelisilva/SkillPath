// Represents the input required to create a new task.
namespace SkillPath.Application.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommand
{
    public Guid GoalId { get; init; }
    public Guid SkillId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Order { get; init; }
}