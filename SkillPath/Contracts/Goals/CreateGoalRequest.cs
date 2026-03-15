// Represents the HTTP request body used to create a goal.
namespace SkillPath.API.Contracts.Goals;

public sealed class CreateGoalRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
