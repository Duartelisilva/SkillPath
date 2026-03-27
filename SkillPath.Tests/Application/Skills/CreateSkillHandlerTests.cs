// Tests for CreateSkillHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Commands.CreateSkill;

namespace SkillPath.Tests.Application.Skills;

public sealed class CreateSkillHandlerTests
{
    private readonly Mock<IGoalRepository> _goalRepository = new();
    private readonly Mock<ISkillRepository> _skillRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateSkillHandler _handler;

    public CreateSkillHandlerTests()
    {
        _handler = new CreateSkillHandler(_goalRepository.Object, _skillRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenGoalExists_ShouldReturnSkillDto()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        _goalRepository.Setup(r => r.GetByIdAsync(goal.Id, CancellationToken.None)).ReturnsAsync(goal);

        var command = new CreateSkillCommand { GoalId = goal.Id, Name = "C# Basics", Description = "Variables and types", Order = 0 };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("C# Basics");
        result.GoalId.Should().Be(goal.Id);
        _skillRepository.Verify(r => r.AddAsync(It.IsAny<Skill>(), CancellationToken.None), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenGoalNotFound_ShouldReturnNull()
    {
        _goalRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync((Goal?)null);

        var result = await _handler.HandleAsync(new CreateSkillCommand { GoalId = Guid.NewGuid(), Name = "C# Basics", Description = "Variables and types", Order = 0 }, CancellationToken.None);

        result.Should().BeNull();
        _skillRepository.Verify(r => r.AddAsync(It.IsAny<Skill>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}