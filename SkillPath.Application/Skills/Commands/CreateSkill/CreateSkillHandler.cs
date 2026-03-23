// Handles the creation of a new skill within a goal.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Dtos;
using SkillPath.Domain.Entities;

namespace SkillPath.Application.Skills.Commands.CreateSkill;

public sealed class CreateSkillHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSkillHandler(IGoalRepository goalRepository, ISkillRepository skillRepository, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _skillRepository = skillRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SkillDto?> HandleAsync(CreateSkillCommand command, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.GoalId, cancellationToken);

        if (goal is null)
            return null;

        var skill = new Skill(command.GoalId, command.Name, command.Description, command.Order);

        await _skillRepository.AddAsync(skill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SkillDto.FromEntity(skill);
    }
}