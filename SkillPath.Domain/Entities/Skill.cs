// Represents a single skill node within a learning goal's skill tree.
using SkillPath.Domain.Common;
using SkillPath.Domain.Enums;
using SkillPath.Domain.Exceptions;

namespace SkillPath.Domain.Entities;

public sealed class Skill : BaseEntity
{
    public Guid GoalId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public SkillStatus Status { get; private set; } = SkillStatus.Locked;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<Guid> _dependsOn = new();
    public IReadOnlyCollection<Guid> DependsOn => _dependsOn.AsReadOnly();

    private readonly List<LearningTask> _tasks = new();
    public IReadOnlyCollection<LearningTask> Tasks => _tasks.AsReadOnly();
    private Skill() { }

    public Skill(Guid goalId, string name, string description, int order)
    {
        GoalId = goalId;
        SetName(name);
        SetDescription(description);
        SetOrder(order);
        Status = SkillStatus.Locked;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void AddDependency(Guid skillId)
    {
        if (skillId == Id)
            throw new DomainException("A skill cannot depend on itself.");

        if (_dependsOn.Contains(skillId))
            return;

        _dependsOn.Add(skillId);
    }

    public void Unlock()
    {
        if (Status == SkillStatus.Available || Status == SkillStatus.InProgress)
            return;

        if (Status == SkillStatus.Completed)
            throw new DomainException("A completed skill cannot be unlocked.");

        Status = SkillStatus.Available;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Start()
    {
        if (Status == SkillStatus.Locked)
            throw new DomainException("A locked skill cannot be started. Complete its dependencies first.");

        if (Status == SkillStatus.InProgress)
            return;

        Status = SkillStatus.InProgress;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status == SkillStatus.Locked)
            throw new DomainException("A locked skill cannot be completed.");

        if (Status == SkillStatus.Completed)
            return;

        Status = SkillStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description)
    {
        SetName(name);
        SetDescription(description);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Skill name is required.");

        if (name.Length > 200)
            throw new DomainException("Skill name cannot exceed 200 characters.");

        Name = name.Trim();
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Skill description is required.");

        if (description.Length > 1000)
            throw new DomainException("Skill description cannot exceed 1000 characters.");

        Description = description.Trim();
    }

    private void SetOrder(int order)
    {
        if (order < 0)
            throw new DomainException("Skill order must be a non-negative number.");

        Order = order;
    }

    public LearningTask AddTask(string title, string description, int order)
    {
        if (Status == SkillStatus.Completed)
            throw new DomainException("Cannot add tasks to a completed skill.");

        var task = new LearningTask(Id, title, description, order);
        _tasks.Add(task);
        UpdatedAtUtc = DateTime.UtcNow;
        return task;
    }
}