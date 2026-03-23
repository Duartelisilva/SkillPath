// Handles deletion of a skill.
using SkillPath.Application.Abstractions.Persistence;

namespace SkillPath.Application.Skills.Commands.DeleteSkill;

public sealed class DeleteSkillHandler
{
    private readonly ISkillRepository _skillRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSkillHandler(ISkillRepository skillRepository, IUnitOfWork unitOfWork)
    {
        _skillRepository = skillRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(DeleteSkillCommand command, CancellationToken cancellationToken)
    {
        var skill = await _skillRepository.GetByIdAsync(command.SkillId, cancellationToken);

        if (skill is null || skill.GoalId != command.GoalId)
            return false;

        await _skillRepository.DeleteAsync(skill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}