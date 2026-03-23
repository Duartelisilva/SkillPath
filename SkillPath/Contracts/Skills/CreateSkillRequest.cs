// Represents the HTTP request body used to create a skill.
namespace SkillPath.API.Contracts.Skills;

public sealed class CreateSkillRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Order { get; init; }
}