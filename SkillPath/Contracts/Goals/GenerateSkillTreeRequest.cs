// Represents the HTTP request body for skill tree generation.
namespace SkillPath.API.Contracts.Goals;

public sealed class GenerateSkillTreeRequest
{
    public string? AdditionalContext { get; init; }
}