// Tests for ListTasksBySkillHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Queries.ListTasksBySkill;

namespace SkillPath.Tests.Application.Tasks;

public sealed class ListTasksBySkillHandlerTests
{
    private readonly Mock<ISkillRepository> _skillRepository = new();
    private readonly Mock<ILearningTaskRepository> _taskRepository = new();
    private readonly ListTasksBySkillHandler _handler;

    public ListTasksBySkillHandlerTests()
    {
        _handler = new ListTasksBySkillHandler(_skillRepository.Object, _taskRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillExistsAndBelongsToGoal_ShouldReturnTasks()
    {
        var goalId = Guid.NewGuid();
        var skill = new Skill(goalId, "C# Basics", "Variables and types", 0);
        var tasks = new List<LearningTask>
        {
            new(skill.Id, "Read docs", "Read the documentation", 0),
            new(skill.Id, "Watch tutorial", "Watch the video", 1)
        };
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);
        _taskRepository.Setup(r => r.ListBySkillAsync(skill.Id, CancellationToken.None)).ReturnsAsync(tasks);

        var result = await _handler.HandleAsync(new ListTasksBySkillQuery { GoalId = goalId, SkillId = skill.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result.Select(t => t.Title).Should().BeEquivalentTo("Read docs", "Watch tutorial");
    }

    [Fact]
    public async Task HandleAsync_WhenSkillBelongsToDifferentGoal_ShouldReturnNull()
    {
        var skill = new Skill(Guid.NewGuid(), "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var result = await _handler.HandleAsync(new ListTasksBySkillQuery { GoalId = Guid.NewGuid(), SkillId = skill.Id }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenSkillNotFound_ShouldReturnNull()
    {
        _skillRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync((Skill?)null);

        var result = await _handler.HandleAsync(new ListTasksBySkillQuery { GoalId = Guid.NewGuid(), SkillId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeNull();
    }
}