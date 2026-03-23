// Represents the HTTP request body used to create a task.
namespace SkillPath.API.Contracts.Tasks;

public sealed class CreateTaskRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Order { get; init; }
}