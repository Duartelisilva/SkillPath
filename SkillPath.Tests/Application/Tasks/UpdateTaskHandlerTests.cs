// Tests for UpdateTaskHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Commands.UpdateTask;

namespace SkillPath.Tests.Application.Tasks;

public sealed class UpdateTaskHandlerTests
{
    private readonly Mock<ISkillRepository> _skillRepository = new();
    private readonly Mock<ILearningTaskRepository> _taskRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateTaskHandler _handler;

    public UpdateTaskHandlerTests()
    {
        _handler = new UpdateTaskHandler(_taskRepository.Object, _skillRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenAllValid_ShouldReturnUpdatedDto()
    {
        var goalId = Guid.NewGuid();
        var skill = new Skill(goalId, "C# Basics", "Variables and types", 0);
        var task = new LearningTask(skill.Id, "Read docs", "Read the documentation", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);
        _taskRepository.Setup(r => r.GetByIdAsync(task.Id, CancellationToken.None)).ReturnsAsync(task);

        var result = await _handler.HandleAsync(new UpdateTaskCommand { GoalId = goalId, SkillId = skill.Id, TaskId = task.Id, Title = "Watch tutorial", Description = "Watch the video" }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Watch tutorial");
        result.Description.Should().Be("Watch the video");
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillBelongsToDifferentGoal_ShouldReturnNull()
    {
        var skill = new Skill(Guid.NewGuid(), "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var result = await _handler.HandleAsync(new UpdateTaskCommand { GoalId = Guid.NewGuid(), SkillId = skill.Id, TaskId = Guid.NewGuid(), Title = "X", Description = "Y" }, CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTaskBelongsToDifferentSkill_ShouldReturnNull()
    {
        var goalId = Guid.NewGuid();
        var skill = new Skill(goalId, "C# Basics", "Variables and types", 0);
        var task = new LearningTask(Guid.NewGuid(), "Read docs", "Read the documentation", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);
        _taskRepository.Setup(r => r.GetByIdAsync(task.Id, CancellationToken.None)).ReturnsAsync(task);

        var result = await _handler.HandleAsync(new UpdateTaskCommand { GoalId = goalId, SkillId = skill.Id, TaskId = task.Id, Title = "X", Description = "Y" }, CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}