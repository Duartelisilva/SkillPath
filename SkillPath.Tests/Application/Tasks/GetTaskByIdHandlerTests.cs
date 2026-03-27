// Tests for GetTaskByIdHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Queries.GetTaskById;

namespace SkillPath.Tests.Application.Tasks;

public sealed class GetTaskByIdHandlerTests
{
    private readonly Mock<ISkillRepository> _skillRepository = new();
    private readonly Mock<ILearningTaskRepository> _taskRepository = new();
    private readonly GetTaskByIdHandler _handler;

    public GetTaskByIdHandlerTests()
    {
        _handler = new GetTaskByIdHandler(_taskRepository.Object, _skillRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenAllValid_ShouldReturnDto()
    {
        var goalId = Guid.NewGuid();
        var skill = new Skill(goalId, "C# Basics", "Variables and types", 0);
        var task = new LearningTask(skill.Id, "Read docs", "Read the documentation", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);
        _taskRepository.Setup(r => r.GetByIdAsync(task.Id, CancellationToken.None)).ReturnsAsync(task);

        var result = await _handler.HandleAsync(new GetTaskByIdQuery { GoalId = goalId, SkillId = skill.Id, TaskId = task.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Title.Should().Be("Read docs");
    }

    [Fact]
    public async Task HandleAsync_WhenSkillBelongsToDifferentGoal_ShouldReturnNull()
    {
        var skill = new Skill(Guid.NewGuid(), "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var result = await _handler.HandleAsync(new GetTaskByIdQuery { GoalId = Guid.NewGuid(), SkillId = skill.Id, TaskId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTaskBelongsToDifferentSkill_ShouldReturnNull()
    {
        var goalId = Guid.NewGuid();
        var skill = new Skill(goalId, "C# Basics", "Variables and types", 0);
        var task = new LearningTask(Guid.NewGuid(), "Read docs", "Read the documentation", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);
        _taskRepository.Setup(r => r.GetByIdAsync(task.Id, CancellationToken.None)).ReturnsAsync(task);

        var result = await _handler.HandleAsync(new GetTaskByIdQuery { GoalId = goalId, SkillId = skill.Id, TaskId = task.Id }, CancellationToken.None);

        result.Should().BeNull();
    }
}