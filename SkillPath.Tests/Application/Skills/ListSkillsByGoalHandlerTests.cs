// Tests for ListSkillsByGoalHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Queries.ListSkillsByGoal;

namespace SkillPath.Tests.Application.Skills;

public sealed class ListSkillsByGoalHandlerTests
{
    private readonly Mock<IGoalRepository> _goalRepository = new();
    private readonly Mock<ISkillRepository> _skillRepository = new();
    private readonly ListSkillsByGoalHandler _handler;

    public ListSkillsByGoalHandlerTests()
    {
        _handler = new ListSkillsByGoalHandler(_goalRepository.Object, _skillRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenGoalExists_ShouldReturnSkills()
    {
        var goal = new Goal("Learn C#", "Master C# fundamentals");
        var skills = new List<Skill>
        {
            new(goal.Id, "C# Basics", "Variables and types", 0),
            new(goal.Id, "OOP", "Classes and interfaces", 1)
        };
        _goalRepository.Setup(r => r.GetByIdAsync(goal.Id, CancellationToken.None)).ReturnsAsync(goal);
        _skillRepository.Setup(r => r.ListByGoalAsync(goal.Id, CancellationToken.None)).ReturnsAsync(skills);

        var result = await _handler.HandleAsync(new ListSkillsByGoalQuery { GoalId = goal.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result.Select(s => s.Name).Should().BeEquivalentTo("C# Basics", "OOP");
    }

    [Fact]
    public async Task HandleAsync_WhenGoalNotFound_ShouldReturnNull()
    {
        _goalRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync((Goal?)null);

        var result = await _handler.HandleAsync(new ListSkillsByGoalQuery { GoalId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeNull();
    }
}