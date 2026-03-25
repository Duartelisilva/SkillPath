// Unit tests for the Goal entity business rules.
global using FluentAssertions;
global using Xunit;
global using SkillPath.Domain.Entities;
global using SkillPath.Domain.Enums;
global using SkillPath.Domain.Exceptions;

namespace SkillPath.Tests.Domain;

public sealed class GoalTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateGoalWithDraftStatus()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");

        goal.Title.Should().Be("Learn C#");
        goal.Description.Should().Be("Master C# fundamentals");
        goal.Status.Should().Be(GoalStatus.Draft);
    }

    [Fact]
    public void Constructor_WithEmptyTitle_ShouldThrowDomainException()
    {
        Action act = () => new Goal("", "Some description");

        act.Should().Throw<DomainException>()
            .WithMessage("Goal title is required.");
    }

    [Fact]
    public void Constructor_WithEmptyDescription_ShouldThrowDomainException()
    {
        Action act = () => new Goal("Learn C#", "");

        act.Should().Throw<DomainException>()
            .WithMessage("Goal description is required.");
    }

    [Fact]
    public void Constructor_WithTitleExceeding200Chars_ShouldThrowDomainException()
    {
        var longTitle = new string('a', 201);

        Action act = () => new Goal(longTitle, "Some description");

        act.Should().Throw<DomainException>()
            .WithMessage("Goal title cannot exceed 200 characters.");
    }

    [Fact]
    public void Activate_WhenDraft_ShouldSetStatusToActive()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");

        goal.Activate();

        goal.Status.Should().Be(GoalStatus.Active);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotThrow()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        goal.Activate();

        Action act = () => goal.Activate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Activate_WhenArchived_ShouldThrowDomainException()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        goal.Archive();

        Action act = () => goal.Activate();

        act.Should().Throw<DomainException>()
            .WithMessage("An archived goal cannot be activated.");
    }

    [Fact]
    public void Complete_WhenActive_ShouldSetStatusToCompleted()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        goal.Activate();

        goal.Complete();

        goal.Status.Should().Be(GoalStatus.Completed);
    }

    [Fact]
    public void Complete_WhenArchived_ShouldThrowDomainException()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        goal.Archive();

        Action act = () => goal.Complete();

        act.Should().Throw<DomainException>()
            .WithMessage("An archived goal cannot be completed.");
    }

    [Fact]
    public void Archive_WhenActive_ShouldSetStatusToArchived()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        goal.Activate();

        goal.Archive();

        goal.Status.Should().Be(GoalStatus.Archived);
    }

    [Fact]
    public void AddSkill_WhenGoalIsNotArchived_ShouldAddSkillToCollection()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");

        var skill = goal.AddSkill("C# Basics", "Variables and types", 1);

        goal.Skills.Should().ContainSingle();
        goal.Skills.First().Name.Should().Be("C# Basics");
    }

    [Fact]
    public void AddSkill_WhenGoalIsArchived_ShouldThrowDomainException()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        goal.Archive();

        Action act = () => goal.AddSkill("C# Basics", "Variables and types", 1);

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot add skills to an archived goal.");
    }

    [Fact]
    public void RemoveSkill_WhenSkillExists_ShouldRemoveFromCollection()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        var skill = goal.AddSkill("C# Basics", "Variables and types", 1);

        goal.RemoveSkill(skill.Id);

        goal.Skills.Should().BeEmpty();
    }

    [Fact]
    public void RemoveSkill_WhenSkillDoesNotExist_ShouldThrowDomainException()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");

        Action act = () => goal.RemoveSkill(Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage("Skill not found in this goal.");
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateTitleAndDescription()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");

        goal.UpdateDetails("Learn C# Advanced", "Deep dive into C#");

        goal.Title.Should().Be("Learn C# Advanced");
        goal.Description.Should().Be("Deep dive into C#");
        goal.UpdatedAtUtc.Should().NotBeNull();
    }
}