// Handles retrieval of a single skill by identifier.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Dtos;

namespace SkillPath.Application.Skills.Queries.GetSkillById;

public sealed class GetSkillByIdHandler
{
    private readonly IGoalRepository _goalRepository;

    public GetSkillByIdHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<SkillDto?> HandleAsync(GetSkillByIdQuery query, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(query.GoalId, cancellationToken);

        if (goal is null)
            return null;

        var skill = goal.Skills.FirstOrDefault(s => s.Id == query.SkillId);

        return skill is null ? null : SkillDto.FromEntity(skill);
    }
}