// Tests for DeleteSkillHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Commands.DeleteSkill;

namespace SkillPath.Tests.Application.Skills;

public sealed class DeleteSkillHandlerTests
{
    private readonly Mock<ISkillRepository> _skillRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteSkillHandler _handler;

    public DeleteSkillHandlerTests()
    {
        _handler = new DeleteSkillHandler(_skillRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillExistsAndBelongsToGoal_ShouldReturnTrue()
    {
        var goalId = Guid.NewGuid();
        var skill = new Skill(goalId, "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var result = await _handler.HandleAsync(new DeleteSkillCommand { GoalId = goalId, SkillId = skill.Id }, CancellationToken.None);

        result.Should().BeTrue();
        _skillRepository.Verify(r => r.DeleteAsync(skill, CancellationToken.None), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillBelongsToDifferentGoal_ShouldReturnFalse()
    {
        var skill = new Skill(Guid.NewGuid(), "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var result = await _handler.HandleAsync(new DeleteSkillCommand { GoalId = Guid.NewGuid(), SkillId = skill.Id }, CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillNotFound_ShouldReturnFalse()
    {
        _skillRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync((Skill?)null);

        var result = await _handler.HandleAsync(new DeleteSkillCommand { GoalId = Guid.NewGuid(), SkillId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeFalse();
    }
}