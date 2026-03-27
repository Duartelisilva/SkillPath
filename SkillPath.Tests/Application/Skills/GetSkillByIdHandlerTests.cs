// Tests for GetSkillByIdHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Queries.GetSkillById;

namespace SkillPath.Tests.Application.Skills;

public sealed class GetSkillByIdHandlerTests
{
    private readonly Mock<ISkillRepository> _skillRepository = new();
    private readonly GetSkillByIdHandler _handler;

    public GetSkillByIdHandlerTests()
    {
        _handler = new GetSkillByIdHandler(_skillRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillExistsAndBelongsToGoal_ShouldReturnDto()
    {
        var goalId = Guid.NewGuid();
        var skill = new Skill(goalId, "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var result = await _handler.HandleAsync(new GetSkillByIdQuery { GoalId = goalId, SkillId = skill.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(skill.Id);
        result.Name.Should().Be("C# Basics");
    }

    [Fact]
    public async Task HandleAsync_WhenSkillNotFound_ShouldReturnNull()
    {
        _skillRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync((Skill?)null);

        var result = await _handler.HandleAsync(new GetSkillByIdQuery { GoalId = Guid.NewGuid(), SkillId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenSkillBelongsToDifferentGoal_ShouldReturnNull()
    {
        var skill = new Skill(Guid.NewGuid(), "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var result = await _handler.HandleAsync(new GetSkillByIdQuery { GoalId = Guid.NewGuid(), SkillId = skill.Id }, CancellationToken.None);

        result.Should().BeNull();
    }
}