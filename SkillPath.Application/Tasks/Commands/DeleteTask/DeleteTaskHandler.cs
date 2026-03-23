// Handles deletion of a task.
using SkillPath.Application.Abstractions.Persistence;

namespace SkillPath.Application.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskHandler
{
    private readonly ILearningTaskRepository _taskRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskHandler(ILearningTaskRepository taskRepository, ISkillRepository skillRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _skillRepository = skillRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        var skill = await _skillRepository.GetByIdAsync(command.SkillId, cancellationToken);

        if (skill is null || skill.GoalId != command.GoalId)
            return false;

        var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

        if (task is null || task.SkillId != command.SkillId)
            return false;

        await _taskRepository.DeleteAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}