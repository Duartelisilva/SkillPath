// Tests for UpdateGoalHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Goals.Commands.UpdateGoal;

namespace SkillPath.Tests.Application.Goals;

public sealed class UpdateGoalHandlerTests
{
    private readonly Mock<IGoalRepository> _goalRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateGoalHandler _handler;

    public UpdateGoalHandlerTests()
    {
        _handler = new UpdateGoalHandler(_goalRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenGoalExists_ShouldReturnUpdatedDto()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        _goalRepository.Setup(r => r.GetByIdAsync(goal.Id, CancellationToken.None)).ReturnsAsync(goal);

        var command = new UpdateGoalCommand { Id = goal.Id, Title = "Learn C# Advanced", Description = "Deep dive into C#" };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Learn C# Advanced");
        result.Description.Should().Be("Deep dive into C#");
    }

    [Fact]
    public async Task HandleAsync_WhenGoalNotFound_ShouldReturnNull()
    {
        _goalRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync((Goal?)null);

        var result = await _handler.HandleAsync(new UpdateGoalCommand { Id = Guid.NewGuid(), Title = "X", Description = "Y" }, CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}