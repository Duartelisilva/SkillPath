// Handles retrieval of all tasks for a given skill.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Dtos;

namespace SkillPath.Application.Tasks.Queries.ListTasksBySkill;

public sealed class ListTasksBySkillHandler
{
    private readonly IGoalRepository _goalRepository;

    public ListTasksBySkillHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<IReadOnlyCollection<LearningTaskDto>?> HandleAsync(ListTasksBySkillQuery query, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(query.GoalId, cancellationToken);

        if (goal is null)
            return null;

        var skill = goal.Skills.FirstOrDefault(s => s.Id == query.SkillId);

        if (skill is null)
            return null;

        return skill.Tasks
            .OrderBy(t => t.Order)
            .Select(LearningTaskDto.FromEntity)
            .ToArray();
    }
}