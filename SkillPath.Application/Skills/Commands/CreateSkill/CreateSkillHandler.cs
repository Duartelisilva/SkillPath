// Handles the creation of a new skill within a goal.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Dtos;

namespace SkillPath.Application.Skills.Commands.CreateSkill;

public sealed class CreateSkillHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSkillHandler(IGoalRepository goalRepository, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SkillDto?> HandleAsync(CreateSkillCommand command, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.GoalId, cancellationToken);

        if (goal is null)
            return null;

        var skill = goal.AddSkill(command.Name, command.Description, command.Order);

        await _goalRepository.UpdateAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SkillDto.FromEntity(skill);
    }
}