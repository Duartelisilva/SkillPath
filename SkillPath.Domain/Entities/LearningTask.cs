// Represents a concrete actionable task within a skill.
using SkillPath.Domain.Common;
using SkillPath.Domain.Enums;
using SkillPath.Domain.Exceptions;

namespace SkillPath.Domain.Entities;

public sealed class LearningTask : BaseEntity
{
    public Guid SkillId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public LearningTaskStatus Status { get; private set; } = LearningTaskStatus.NotStarted;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public int ExperiencePoints { get; private set; }

    private LearningTask() { }

    public LearningTask(Guid skillId, string title, string description, int order, int experiencePoints = 0)
    {
        SkillId = skillId;
        SetTitle(title);
        SetDescription(description);
        SetOrder(order);
        SetExperiencePoints(experiencePoints);
        Status = LearningTaskStatus.NotStarted;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Start()
    {
        if (Status == LearningTaskStatus.InProgress)
            return;

        if (Status == LearningTaskStatus.Completed)
            throw new DomainException("A completed task cannot be restarted.");

        Status = LearningTaskStatus.InProgress;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status == LearningTaskStatus.Completed)
            return;

        Status = LearningTaskStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Reset()
    {
        if (Status == LearningTaskStatus.NotStarted)
            return;

        Status = LearningTaskStatus.NotStarted;
        CompletedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ZeroExperiencePoints()
    {
        ExperiencePoints = 0;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetExperiencePoints(int experiencePoints)
    {
        if (experiencePoints < 0)
            throw new DomainException("Experience points must be non-negative.");

        ExperiencePoints = experiencePoints;
    }

    public void UpdateDetails(string title, string description)
    {
        SetTitle(title);
        SetDescription(description);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Task title is required.");

        if (title.Length > 200)
            throw new DomainException("Task title cannot exceed 200 characters.");

        Title = title.Trim();
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Task description is required.");

        if (description.Length > 1000)
            throw new DomainException("Task description cannot exceed 1000 characters.");

        Description = description.Trim();
    }

    private void SetOrder(int order)
    {
        if (order < 0)
            throw new DomainException("Task order must be a non-negative number.");

        Order = order;
    }
}