using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Goals.Dtos;

namespace SkillPath.Application.Goals.Commands.ActivateGoal;

public sealed class ActivateGoalHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateGoalHandler(IGoalRepository goalRepository, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<GoalDto?> HandleAsync(ActivateGoalCommand command, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.Id, cancellationToken);

        if (goal is null)
            return null;

        goal.Activate();

        await _goalRepository.UpdateAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GoalDto.FromEntity(goal);
    }
}