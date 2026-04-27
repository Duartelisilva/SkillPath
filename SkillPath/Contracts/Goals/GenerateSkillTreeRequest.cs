// Represents the HTTP request body for skill tree generation.
namespace SkillPath.API.Contracts.Goals;

public sealed class GenerateSkillTreeRequest
{
    public string? AdditionalContext { get; init; }

    // Generation parameters (optional - have defaults)
    public int? MinSkills { get; init; }
    public int? MaxSkills { get; init; }
    public int? TasksPerSkill { get; init; }
    public string? Difficulty { get; init; } // "Beginner", "Intermediate", "Advanced"
    public string? Focus { get; init; } // "Breadth", "Depth", "Balanced"
}