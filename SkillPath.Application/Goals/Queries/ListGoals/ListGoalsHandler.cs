// Handles retrieval of all goals.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Goals.Dtos;

namespace SkillPath.Application.Goals.Queries.ListGoals;

public sealed class ListGoalsHandler
{
    private readonly IGoalRepository _goalRepository;

    public ListGoalsHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<IReadOnlyCollection<GoalDto>> HandleAsync(ListGoalsQuery query, CancellationToken cancellationToken)
    {
        var goals = await _goalRepository.ListAsync(cancellationToken);

        return goals
            .Select(GoalDto.FromEntity)
            .ToArray();
    }
}
