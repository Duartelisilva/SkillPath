// Handles the creation of a new goal.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Goals.Dtos;
using SkillPath.Domain.Entities;

namespace SkillPath.Application.Goals.Commands.CreateGoal;

public sealed class CreateGoalHandler
{
    private readonly IGoalRepository _goalRepository;

    public CreateGoalHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<GoalDto> HandleAsync(CreateGoalCommand command, CancellationToken cancellationToken)
    {
        var goal = new Goal(command.Title, command.Description);

        await _goalRepository.AddAsync(goal, cancellationToken);

        return GoalDto.FromEntity(goal);
    }
}
