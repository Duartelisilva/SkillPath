// Represents the HTTP request body used to update a task.
namespace SkillPath.API.Contracts.Tasks;

public sealed class UpdateTaskRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}