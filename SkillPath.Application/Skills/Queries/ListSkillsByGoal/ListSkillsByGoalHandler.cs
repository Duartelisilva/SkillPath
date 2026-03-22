// Handles retrieval of all skills for a given goal.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Dtos;

namespace SkillPath.Application.Skills.Queries.ListSkillsByGoal;

public sealed class ListSkillsByGoalHandler
{
    private readonly IGoalRepository _goalRepository;

    public ListSkillsByGoalHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<IReadOnlyCollection<SkillDto>?> HandleAsync(ListSkillsByGoalQuery query, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(query.GoalId, cancellationToken);

        if (goal is null)
            return null;

        return goal.Skills
            .OrderBy(s => s.Order)
            .Select(SkillDto.FromEntity)
            .ToArray();
    }
}