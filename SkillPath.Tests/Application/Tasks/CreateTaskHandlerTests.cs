// Tests for CreateTaskHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Commands.CreateTask;

namespace SkillPath.Tests.Application.Tasks;

public sealed class CreateTaskHandlerTests
{
    private readonly Mock<ISkillRepository> _skillRepository = new();
    private readonly Mock<ILearningTaskRepository> _taskRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateTaskHandler _handler;

    public CreateTaskHandlerTests()
    {
        _handler = new CreateTaskHandler(_skillRepository.Object, _taskRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillExistsAndBelongsToGoal_ShouldReturnTaskDto()
    {
        var goalId = Guid.NewGuid();
        var skill = new Skill(goalId, "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var command = new CreateTaskCommand { GoalId = goalId, SkillId = skill.Id, Title = "Read docs", Description = "Read the documentation", Order = 0 };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Read docs");
        result.SkillId.Should().Be(skill.Id);
        _taskRepository.Verify(r => r.AddAsync(It.IsAny<LearningTask>(), CancellationToken.None), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillBelongsToDifferentGoal_ShouldReturnNull()
    {
        var skill = new Skill(Guid.NewGuid(), "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var result = await _handler.HandleAsync(new CreateTaskCommand { GoalId = Guid.NewGuid(), SkillId = skill.Id, Title = "Read docs", Description = "Read the documentation", Order = 0 }, CancellationToken.None);

        result.Should().BeNull();
        _taskRepository.Verify(r => r.AddAsync(It.IsAny<LearningTask>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillNotFound_ShouldReturnNull()
    {
        _skillRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync((Skill?)null);

        var result = await _handler.HandleAsync(new CreateTaskCommand { GoalId = Guid.NewGuid(), SkillId = Guid.NewGuid(), Title = "Read docs", Description = "Read the documentation", Order = 0 }, CancellationToken.None);

        result.Should().BeNull();
    }
}