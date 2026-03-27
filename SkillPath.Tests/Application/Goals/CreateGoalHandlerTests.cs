// Tests for CreateGoalHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Goals.Commands.CreateGoal;

namespace SkillPath.Tests.Application.Goals;

public sealed class CreateGoalHandlerTests
{
    private readonly Mock<IGoalRepository> _goalRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateGoalHandler _handler;

    public CreateGoalHandlerTests()
    {
        _handler = new CreateGoalHandler(_goalRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnGoalDto()
    {
        var command = new CreateGoalCommand { Title = "Learn C#", Description = "Master C# fundamentals" };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Learn C#");
        result.Description.Should().Be("Master C# fundamentals");
        result.Status.Should().Be("Draft");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCallAddAndSave()
    {
        var command = new CreateGoalCommand { Title = "Learn C#", Description = "Master C# fundamentals" };

        await _handler.HandleAsync(command, CancellationToken.None);

        _goalRepository.Verify(r => r.AddAsync(It.IsAny<Goal>(), CancellationToken.None), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}