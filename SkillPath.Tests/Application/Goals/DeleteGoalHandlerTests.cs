// Tests for DeleteGoalHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Goals.Commands.DeleteGoal;

namespace SkillPath.Tests.Application.Goals;

public sealed class DeleteGoalHandlerTests
{
    private readonly Mock<IGoalRepository> _goalRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteGoalHandler _handler;

    public DeleteGoalHandlerTests()
    {
        _handler = new DeleteGoalHandler(_goalRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenGoalExists_ShouldReturnTrue()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        _goalRepository.Setup(r => r.GetByIdAsync(goal.Id, CancellationToken.None)).ReturnsAsync(goal);

        var result = await _handler.HandleAsync(new DeleteGoalCommand { Id = goal.Id }, CancellationToken.None);

        result.Should().BeTrue();
        _goalRepository.Verify(r => r.DeleteAsync(goal, CancellationToken.None), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenGoalNotFound_ShouldReturnFalse()
    {
        _goalRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync((Goal?)null);

        var result = await _handler.HandleAsync(new DeleteGoalCommand { Id = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}