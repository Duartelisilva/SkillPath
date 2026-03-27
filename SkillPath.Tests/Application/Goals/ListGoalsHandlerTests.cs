// Tests for ListGoalsHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Goals.Queries.ListGoals;

namespace SkillPath.Tests.Application.Goals;

public sealed class ListGoalsHandlerTests
{
    private readonly Mock<IGoalRepository> _goalRepository = new();
    private readonly ListGoalsHandler _handler;

    public ListGoalsHandlerTests()
    {
        _handler = new ListGoalsHandler(_goalRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllGoals()
    {
        var goals = new List<Goal>
        {
            new("Learn C#", "Master C# fundamentals"),
            new("Learn Azure", "Master Azure cloud services")
        };
        _goalRepository.Setup(r => r.ListAsync(CancellationToken.None)).ReturnsAsync(goals);

        var result = await _handler.HandleAsync(new ListGoalsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(g => g.Title).Should().BeEquivalentTo("Learn C#", "Learn Azure");
    }

    [Fact]
    public async Task HandleAsync_WhenNoGoals_ShouldReturnEmptyCollection()
    {
        _goalRepository.Setup(r => r.ListAsync(CancellationToken.None)).ReturnsAsync(new List<Goal>());

        var result = await _handler.HandleAsync(new ListGoalsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}