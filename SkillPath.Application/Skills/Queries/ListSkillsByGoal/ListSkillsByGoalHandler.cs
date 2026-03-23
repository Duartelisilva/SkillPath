// Handles retrieval of all skills for a given goal.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Dtos;

namespace SkillPath.Application.Skills.Queries.ListSkillsByGoal;

public sealed class ListSkillsByGoalHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly ISkillRepository _skillRepository;

    public ListSkillsByGoalHandler(IGoalRepository goalRepository, ISkillRepository skillRepository)
    {
        _goalRepository = goalRepository;
        _skillRepository = skillRepository;
    }

    public async Task<IReadOnlyCollection<SkillDto>?> HandleAsync(ListSkillsByGoalQuery query, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(query.GoalId, cancellationToken);

        if (goal is null)
            return null;

        var skills = await _skillRepository.ListByGoalAsync(query.GoalId, cancellationToken);

        return skills
            .Select(SkillDto.FromEntity)
            .ToArray();
    }
}