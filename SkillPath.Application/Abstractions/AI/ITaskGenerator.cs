// Abstraction for AI-powered task generation for a skill.
namespace SkillPath.Application.Abstractions.AI;

public sealed record GeneratedTask
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Order { get; init; }
}

public interface ITaskGenerator
{
    Task<IReadOnlyCollection<GeneratedTask>> GenerateAsync(
        string skillName,
        string skillDescription,
        string goalTitle,
        CancellationToken cancellationToken);
}