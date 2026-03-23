// Handles retrieval of a single skill by identifier.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Dtos;

namespace SkillPath.Application.Skills.Queries.GetSkillById;

public sealed class GetSkillByIdHandler
{
    private readonly ISkillRepository _skillRepository;

    public GetSkillByIdHandler(ISkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public async Task<SkillDto?> HandleAsync(GetSkillByIdQuery query, CancellationToken cancellationToken)
    {
        var skill = await _skillRepository.GetByIdAsync(query.SkillId, cancellationToken);

        if (skill is null || skill.GoalId != query.GoalId)
            return null;

        return SkillDto.FromEntity(skill);
    }
}