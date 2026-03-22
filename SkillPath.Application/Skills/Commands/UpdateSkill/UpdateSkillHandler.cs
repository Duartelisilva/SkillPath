// Handles updates to an existing skill.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Dtos;

namespace SkillPath.Application.Skills.Commands.UpdateSkill;

public sealed class UpdateSkillHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSkillHandler(IGoalRepository goalRepository, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SkillDto?> HandleAsync(UpdateSkillCommand command, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.GoalId, cancellationToken);

        if (goal is null)
            return null;

        var skill = goal.Skills.FirstOrDefault(s => s.Id == command.SkillId);

        if (skill is null)
            return null;

        skill.UpdateDetails(command.Name, command.Description);

        await _goalRepository.UpdateAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SkillDto.FromEntity(skill);
    }
}