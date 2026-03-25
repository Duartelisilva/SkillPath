// Unit tests for the LearningTask entity business rules.
using SkillPath.Domain.Entities;
using SkillPath.Domain.Enums;
using SkillPath.Domain.Exceptions;

namespace SkillPath.Tests.Domain;

public sealed class LearningTaskTests
{
    private static LearningTask CreateTask() =>
        new(Guid.NewGuid(), "Read docs", "Read the documentation", 1);

    [Fact]
    public void Constructor_WithValidData_ShouldCreateTaskWithNotStartedStatus()
    {
        var task = CreateTask();

        task.Title.Should().Be("Read docs");
        task.Status.Should().Be(LearningTaskStatus.NotStarted);
        task.CompletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyTitle_ShouldThrowDomainException()
    {
        Action act = () => new LearningTask(Guid.NewGuid(), "", "Some description", 1);

        act.Should().Throw<DomainException>()
            .WithMessage("Task title is required.");
    }

    [Fact]
    public void Constructor_WithNegativeOrder_ShouldThrowDomainException()
    {
        Action act = () => new LearningTask(Guid.NewGuid(), "Read docs", "Some description", -1);

        act.Should().Throw<DomainException>()
            .WithMessage("Task order must be a non-negative number.");
    }

    [Fact]
    public void Start_WhenNotStarted_ShouldSetStatusToInProgress()
    {
        var task = CreateTask();

        task.Start();

        task.Status.Should().Be(LearningTaskStatus.InProgress);
    }

    [Fact]
    public void Start_WhenAlreadyInProgress_ShouldNotThrow()
    {
        var task = CreateTask();
        task.Start();

        Action act = () => task.Start();

        act.Should().NotThrow();
    }

    [Fact]
    public void Start_WhenCompleted_ShouldThrowDomainException()
    {
        var task = CreateTask();
        task.Complete();

        Action act = () => task.Start();

        act.Should().Throw<DomainException>()
            .WithMessage("A completed task cannot be restarted.");
    }

    [Fact]
    public void Complete_WhenNotStarted_ShouldSetStatusToCompleted()
    {
        var task = CreateTask();

        task.Complete();

        task.Status.Should().Be(LearningTaskStatus.Completed);
        task.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldNotThrow()
    {
        var task = CreateTask();
        task.Complete();

        Action act = () => task.Complete();

        act.Should().NotThrow();
    }

    [Fact]
    public void Reset_WhenCompleted_ShouldSetStatusToNotStarted()
    {
        var task = CreateTask();
        task.Complete();

        task.Reset();

        task.Status.Should().Be(LearningTaskStatus.NotStarted);
        task.CompletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Reset_WhenNotStarted_ShouldNotThrow()
    {
        var task = CreateTask();

        Action act = () => task.Reset();

        act.Should().NotThrow();
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateTitleAndDescription()
    {
        var task = CreateTask();

        task.UpdateDetails("Watch tutorial", "Watch the YouTube tutorial");

        task.Title.Should().Be("Watch tutorial");
        task.Description.Should().Be("Watch the YouTube tutorial");
        task.UpdatedAtUtc.Should().NotBeNull();
    }
}