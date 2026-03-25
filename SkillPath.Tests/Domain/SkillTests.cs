// Unit tests for the Skill entity business rules.
using SkillPath.Domain.Entities;
using SkillPath.Domain.Enums;
using SkillPath.Domain.Exceptions;

namespace SkillPath.Tests.Domain;

public sealed class SkillTests
{
    private static Skill CreateSkill() =>
        new(Guid.NewGuid(), "C# Basics", "Variables and types", 1);

    [Fact]
    public void Constructor_WithValidData_ShouldCreateSkillWithLockedStatus()
    {
        var skill = CreateSkill();

        skill.Name.Should().Be("C# Basics");
        skill.Status.Should().Be(SkillStatus.Locked);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowDomainException()
    {
        Action act = () => new Skill(Guid.NewGuid(), "", "Some description", 1);

        act.Should().Throw<DomainException>()
            .WithMessage("Skill name is required.");
    }

    [Fact]
    public void Constructor_WithNegativeOrder_ShouldThrowDomainException()
    {
        Action act = () => new Skill(Guid.NewGuid(), "C# Basics", "Some description", -1);

        act.Should().Throw<DomainException>()
            .WithMessage("Skill order must be a non-negative number.");
    }

    [Fact]
    public void Unlock_WhenLocked_ShouldSetStatusToAvailable()
    {
        var skill = CreateSkill();

        skill.Unlock();

        skill.Status.Should().Be(SkillStatus.Available);
    }

    [Fact]
    public void Unlock_WhenCompleted_ShouldThrowDomainException()
    {
        var skill = CreateSkill();
        skill.Unlock();
        skill.Complete();

        Action act = () => skill.Unlock();

        act.Should().Throw<DomainException>()
            .WithMessage("A completed skill cannot be unlocked.");
    }

    [Fact]
    public void Start_WhenAvailable_ShouldSetStatusToInProgress()
    {
        var skill = CreateSkill();
        skill.Unlock();

        skill.Start();

        skill.Status.Should().Be(SkillStatus.InProgress);
    }

    [Fact]
    public void Start_WhenLocked_ShouldThrowDomainException()
    {
        var skill = CreateSkill();

        Action act = () => skill.Start();

        act.Should().Throw<DomainException>()
            .WithMessage("A locked skill cannot be started. Complete its dependencies first.");
    }

    [Fact]
    public void Complete_WhenAvailable_ShouldSetStatusToCompleted()
    {
        var skill = CreateSkill();
        skill.Unlock();

        skill.Complete();

        skill.Status.Should().Be(SkillStatus.Completed);
    }

    [Fact]
    public void Complete_WhenLocked_ShouldThrowDomainException()
    {
        var skill = CreateSkill();

        Action act = () => skill.Complete();

        act.Should().Throw<DomainException>()
            .WithMessage("A locked skill cannot be completed.");
    }

    [Fact]
    public void AddDependency_WithValidSkillId_ShouldAddToDependsOn()
    {
        var skill = CreateSkill();
        var dependencyId = Guid.NewGuid();

        skill.AddDependency(dependencyId);

        skill.DependsOn.Should().ContainSingle()
            .Which.Should().Be(dependencyId);
    }

    [Fact]
    public void AddDependency_WithSelfId_ShouldThrowDomainException()
    {
        var skill = CreateSkill();

        Action act = () => skill.AddDependency(skill.Id);

        act.Should().Throw<DomainException>()
            .WithMessage("A skill cannot depend on itself.");
    }

    [Fact]
    public void AddDependency_WithDuplicateId_ShouldNotAddTwice()
    {
        var skill = CreateSkill();
        var dependencyId = Guid.NewGuid();

        skill.AddDependency(dependencyId);
        skill.AddDependency(dependencyId);

        skill.DependsOn.Should().ContainSingle();
    }

    [Fact]
    public void AddTask_WhenNotCompleted_ShouldAddTaskToCollection()
    {
        var skill = CreateSkill();

        skill.AddTask("Read docs", "Read the documentation", 1);

        skill.Tasks.Should().ContainSingle();
    }

    [Fact]
    public void AddTask_WhenCompleted_ShouldThrowDomainException()
    {
        var skill = CreateSkill();
        skill.Unlock();
        skill.Complete();

        Action act = () => skill.AddTask("Read docs", "Read the documentation", 1);

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot add tasks to a completed skill.");
    }

    [Fact]
    public void RemoveTask_WhenTaskExists_ShouldRemoveFromCollection()
    {
        var skill = CreateSkill();
        var task = skill.AddTask("Read docs", "Read the documentation", 1);

        skill.RemoveTask(task.Id);

        skill.Tasks.Should().BeEmpty();
    }

    [Fact]
    public void RemoveTask_WhenTaskDoesNotExist_ShouldThrowDomainException()
    {
        var skill = CreateSkill();

        Action act = () => skill.RemoveTask(Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage("Task not found in this skill.");
    }
}