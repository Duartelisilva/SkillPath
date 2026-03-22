// Represents a learning goal with title, description, and lifecycle rules.
using SkillPath.Domain.Common;
using SkillPath.Domain.Enums;
using SkillPath.Domain.Exceptions;

namespace SkillPath.Domain.Entities;

public sealed class Goal : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public GoalStatus Status { get; private set; } = GoalStatus.Draft;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }
    private readonly List<Skill> _skills = new();
    public IReadOnlyCollection<Skill> Skills => _skills.AsReadOnly();

    private Goal()
    {
    }

    public Goal(string title, string description)
    {
        SetTitle(title);
        SetDescription(description);
        Status = GoalStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string description)
    {
        SetTitle(title);
        SetDescription(description);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == GoalStatus.Active)
        {
            return;
        }

        if (Status == GoalStatus.Archived)
        {
            throw new DomainException("An archived goal cannot be activated.");
        }

        Status = GoalStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status == GoalStatus.Completed)
        {
            return;
        }

        if (Status == GoalStatus.Archived)
        {
            throw new DomainException("An archived goal cannot be completed.");
        }


        Status = GoalStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status == GoalStatus.Archived)
        {
            return;
        }

        Status = GoalStatus.Archived;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Goal title is required.");
        }

        if (title.Length > 200)
        {
            throw new DomainException("Goal title cannot exceed 200 characters.");
        }

        Title = title.Trim();
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Goal description is required.");
        }

        if (description.Length > 2000)
        {
            throw new DomainException("Goal description cannot exceed 2000 characters.");
        }

        Description = description.Trim();
    }

    public Skill AddSkill(string name, string description, int order)
    {
        if (Status == GoalStatus.Archived)
            throw new DomainException("Cannot add skills to an archived goal.");

        var skill = new Skill(Id, name, description, order);
        _skills.Add(skill);
        UpdatedAtUtc = DateTime.UtcNow;
        return skill;
    }

    public void RemoveSkill(Guid skillId)
    {
        if (Status == GoalStatus.Archived)
            throw new DomainException("Cannot remove skills from an archived goal.");

        var skill = _skills.FirstOrDefault(s => s.Id == skillId);

        if (skill is null)
            throw new DomainException("Skill not found in this goal.");

        _skills.Remove(skill);
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
