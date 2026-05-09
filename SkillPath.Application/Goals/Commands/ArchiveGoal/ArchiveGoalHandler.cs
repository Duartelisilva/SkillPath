using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Goals.Dtos;

namespace SkillPath.Application.Goals.Commands.ArchiveGoal;

public sealed class ArchiveGoalHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveGoalHandler(IGoalRepository goalRepository, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<GoalDto?> HandleAsync(ArchiveGoalCommand command, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.Id, cancellationToken);

        if (goal is null)
            return null;

        goal.Archive();

        await _goalRepository.UpdateAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GoalDto.FromEntity(goal);
    }
}