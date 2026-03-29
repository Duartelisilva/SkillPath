// Represents the data returned to clients for a goal.
using SkillPath.Domain.Entities;

namespace SkillPath.Application.Goals.Dtos;

public sealed class GoalDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int SkillCount { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }

    public static GoalDto FromEntity(Goal goal) => new()
    {
        Id = goal.Id,
        Title = goal.Title,
        Description = goal.Description,
        Status = goal.Status.ToString(),
        SkillCount = goal.Skills.Count,
        CreatedAtUtc = goal.CreatedAtUtc,
        UpdatedAtUtc = goal.UpdatedAtUtc
    };
}
