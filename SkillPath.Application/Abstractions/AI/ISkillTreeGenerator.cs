// Abstraction for AI-powered skill tree generation.
namespace SkillPath.Application.Abstractions.AI;

public sealed record GeneratedSkill
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Order { get; init; }
}

public interface ISkillTreeGenerator
{
    Task<IReadOnlyCollection<GeneratedSkill>> GenerateAsync(
        string goalTitle,
        string goalDescription,
        IReadOnlyCollection<string> existingSkillNames,
        CancellationToken cancellationToken);
}