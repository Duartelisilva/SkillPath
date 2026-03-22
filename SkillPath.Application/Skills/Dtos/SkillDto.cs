// Represents the data returned to clients for a skill.
using SkillPath.Domain.Entities;

namespace SkillPath.Application.Skills.Dtos;

public sealed class SkillDto
{
    public Guid Id { get; init; }
    public Guid GoalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Order { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public IReadOnlyCollection<Guid> DependsOn { get; init; } = [];

    public static SkillDto FromEntity(Skill skill) => new()
    {
        Id = skill.Id,
        GoalId = skill.GoalId,
        Name = skill.Name,
        Description = skill.Description,
        Order = skill.Order,
        Status = skill.Status.ToString(),
        CreatedAtUtc = skill.CreatedAtUtc,
        UpdatedAtUtc = skill.UpdatedAtUtc,
        DependsOn = skill.DependsOn
    };
}