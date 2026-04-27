// Represents the input required to generate a skill tree for a goal.
namespace SkillPath.Application.Goals.Commands.GenerateSkillTree;

public sealed class GenerateSkillTreeCommand
{
    public Guid GoalId { get; init; }
    public string? AdditionalContext { get; init; }

    // Generation parameters
    public int MinSkills { get; init; } = 5;
    public int MaxSkills { get; init; } = 12;
    public int TasksPerSkill { get; init; } = 5;
    public DifficultyLevel Difficulty { get; init; } = DifficultyLevel.Intermediate;
    public TreeFocus Focus { get; init; } = TreeFocus.Balanced;
}

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}

public enum TreeFocus
{
    Breadth,
    Depth,
    Balanced
}