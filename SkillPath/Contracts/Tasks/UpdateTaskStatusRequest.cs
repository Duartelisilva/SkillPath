// Represents the HTTP request body used to update a task's status.
namespace SkillPath.API.Contracts.Tasks;

public sealed class UpdateTaskStatusRequest
{
    public string Status { get; init; } = string.Empty; // "NotStarted", "InProgress", "Completed"
}