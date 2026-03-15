// Handles retrieval of a single goal by identifier.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Goals.Dtos;

namespace SkillPath.Application.Goals.Queries.GetGoalById;

public sealed class GetGoalByIdHandler
{
    private readonly IGoalRepository _goalRepository;

    public GetGoalByIdHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<GoalDto?> HandleAsync(GetGoalByIdQuery query, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(query.Id, cancellationToken);

        return goal is null ? null : GoalDto.FromEntity(goal);
    }
}
