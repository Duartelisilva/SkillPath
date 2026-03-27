// Tests for GetGoalByIdHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Goals.Queries.GetGoalById;

namespace SkillPath.Tests.Application.Goals;

public sealed class GetGoalByIdHandlerTests
{
    private readonly Mock<IGoalRepository> _goalRepository = new();
    private readonly GetGoalByIdHandler _handler;

    public GetGoalByIdHandlerTests()
    {
        _handler = new GetGoalByIdHandler(_goalRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenGoalExists_ShouldReturnDto()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        _goalRepository.Setup(r => r.GetByIdAsync(goal.Id, CancellationToken.None)).ReturnsAsync(goal);

        var result = await _handler.HandleAsync(new GetGoalByIdQuery { Id = goal.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(goal.Id);
        result.Title.Should().Be("Learn C#");
    }

    [Fact]
    public async Task HandleAsync_WhenGoalNotFound_ShouldReturnNull()
    {
        _goalRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync((Goal?)null);

        var result = await _handler.HandleAsync(new GetGoalByIdQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeNull();
    }
}