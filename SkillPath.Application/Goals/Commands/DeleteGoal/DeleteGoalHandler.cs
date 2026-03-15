// Handles deletion of an existing goal.
using SkillPath.Application.Abstractions.Persistence;

namespace SkillPath.Application.Goals.Commands.DeleteGoal;

public sealed class DeleteGoalHandler
{
    private readonly IGoalRepository _goalRepository;

    public DeleteGoalHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<bool> HandleAsync(DeleteGoalCommand command, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.Id, cancellationToken);

        if (goal is null)
        {
            return false;
        }

        await _goalRepository.DeleteAsync(goal, cancellationToken);
        return true;
    }
}
