// Handles updates to an existing skill.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Dtos;

namespace SkillPath.Application.Skills.Commands.UpdateSkill;

public sealed class UpdateSkillHandler
{
    private readonly ISkillRepository _skillRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSkillHandler(ISkillRepository skillRepository, IUnitOfWork unitOfWork)
    {
        _skillRepository = skillRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SkillDto?> HandleAsync(UpdateSkillCommand command, CancellationToken cancellationToken)
    {
        var skill = await _skillRepository.GetByIdAsync(command.SkillId, cancellationToken);

        if (skill is null || skill.GoalId != command.GoalId)
            return null;

        skill.UpdateDetails(command.Name, command.Description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SkillDto.FromEntity(skill);
    }
}