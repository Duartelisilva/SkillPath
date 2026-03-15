// Handles updates to an existing goal.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Goals.Dtos;

namespace SkillPath.Application.Goals.Commands.UpdateGoal;

public sealed class UpdateGoalHandler
{
    private readonly IGoalRepository _goalRepository;

    public UpdateGoalHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<GoalDto?> HandleAsync(UpdateGoalCommand command, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.Id, cancellationToken);

        if (goal is null)
        {
            return null;
        }

        goal.UpdateDetails(command.Title, command.Description);

        await _goalRepository.UpdateAsync(goal, cancellationToken);

        return GoalDto.FromEntity(goal);
    }
}
