// Handles deletion of a skill from a goal.
using SkillPath.Application.Abstractions.Persistence;

namespace SkillPath.Application.Skills.Commands.DeleteSkill;

public sealed class DeleteSkillHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSkillHandler(IGoalRepository goalRepository, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(DeleteSkillCommand command, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.GoalId, cancellationToken);

        if (goal is null)
            return false;

        goal.RemoveSkill(command.SkillId);

        await _goalRepository.UpdateAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}