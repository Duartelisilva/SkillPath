// Represents the data returned to clients for a learning task.
using SkillPath.Domain.Entities;

namespace SkillPath.Application.Tasks.Dtos;

public sealed class LearningTaskDto
{
    public Guid Id { get; init; }
    public Guid SkillId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Order { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }

    public static LearningTaskDto FromEntity(LearningTask task) => new()
    {
        Id = task.Id,
        SkillId = task.SkillId,
        Title = task.Title,
        Description = task.Description,
        Order = task.Order,
        Status = task.Status.ToString(),
        CreatedAtUtc = task.CreatedAtUtc,
        UpdatedAtUtc = task.UpdatedAtUtc,
        CompletedAtUtc = task.CompletedAtUtc
    };
}