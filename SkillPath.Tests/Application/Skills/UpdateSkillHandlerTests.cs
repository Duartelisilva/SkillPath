// Tests for UpdateSkillHandler.
using Moq;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Commands.UpdateSkill;

namespace SkillPath.Tests.Application.Skills;

public sealed class UpdateSkillHandlerTests
{
    private readonly Mock<ISkillRepository> _skillRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateSkillHandler _handler;

    public UpdateSkillHandlerTests()
    {
        _handler = new UpdateSkillHandler(_skillRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillExistsAndBelongsToGoal_ShouldReturnUpdatedDto()
    {
        var goalId = Guid.NewGuid();
        var skill = new Skill(goalId, "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var result = await _handler.HandleAsync(new UpdateSkillCommand { GoalId = goalId, SkillId = skill.Id, Name = "C# Advanced", Description = "Deep dive" }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("C# Advanced");
        result.Description.Should().Be("Deep dive");
        _unitOfWork.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillNotFound_ShouldReturnNull()
    {
        _skillRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync((Skill?)null);

        var result = await _handler.HandleAsync(new UpdateSkillCommand { GoalId = Guid.NewGuid(), SkillId = Guid.NewGuid(), Name = "X", Description = "Y" }, CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSkillBelongsToDifferentGoal_ShouldReturnNull()
    {
        var skill = new Skill(Guid.NewGuid(), "C# Basics", "Variables and types", 0);
        _skillRepository.Setup(r => r.GetByIdAsync(skill.Id, CancellationToken.None)).ReturnsAsync(skill);

        var result = await _handler.HandleAsync(new UpdateSkillCommand { GoalId = Guid.NewGuid(), SkillId = skill.Id, Name = "X", Description = "Y" }, CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}