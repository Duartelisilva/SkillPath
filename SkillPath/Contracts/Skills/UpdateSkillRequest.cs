// Represents the HTTP request body used to update a skill.
namespace SkillPath.API.Contracts.Skills;

public sealed class UpdateSkillRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}