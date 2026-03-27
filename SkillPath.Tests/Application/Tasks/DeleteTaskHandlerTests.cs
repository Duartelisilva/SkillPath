// Tests for DeleteTaskHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Commands.DeleteTask;

namespace SkillPath.Tests.Application.Tasks;

public sealed class DeleteTaskHandlerTests
{
    private readonly Mock<ISkillRepository> _skillRepository = new();
    private readonly Mock<ILearningTaskRepository> _taskRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteTaskHandler _handler;

    public DeleteTaskHandlerTests()
    {
        _handler = new DeleteTaskHandler(_taskRepository.Object, _skillRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenAllValid_ShouldReturnTrue()
    {
        var goalId = Guid.NewGuid();
        var skill = new Skill(goalId, "C# Basics", "Variables and types", 0);
        var task = new LearningTask(skill.Id, "Read docs", "Read the documentation", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);
        _taskRepository.Setup(r => r.GetByIdAsync(task.Id, CancellationToken.None)).ReturnsAsync(task);

        var result = await _handler.HandleAsync(new DeleteTaskCommand { GoalId = goalId, SkillId = skill.Id, TaskId = task.Id }, CancellationToken.None);

        result.Should().BeTrue();
        _taskRepository.Verify(r => r.DeleteAsync(task, CancellationToken.None), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillBelongsToDifferentGoal_ShouldReturnFalse()
    {
        var skill = new Skill(Guid.NewGuid(), "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var result = await _handler.HandleAsync(new DeleteTaskCommand { GoalId = Guid.NewGuid(), SkillId = skill.Id, TaskId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTaskBelongsToDifferentSkill_ShouldReturnFalse()
    {
        var goalId = Guid.NewGuid();
        var skill = new Skill(goalId, "C# Basics", "Variables and types", 0);
        var task = new LearningTask(Guid.NewGuid(), "Read docs", "Read the documentation", 0); // different skillId
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);
        _taskRepository.Setup(r => r.GetByIdAsync(task.Id, CancellationToken.None)).ReturnsAsync(task);

        var result = await _handler.HandleAsync(new DeleteTaskCommand { GoalId = goalId, SkillId = skill.Id, TaskId = task.Id }, CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}