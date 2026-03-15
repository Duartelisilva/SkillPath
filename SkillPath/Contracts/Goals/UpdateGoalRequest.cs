// Represents the HTTP request body used to update a goal.
namespace SkillPath.API.Contracts.Goals;

public sealed class UpdateGoalRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
