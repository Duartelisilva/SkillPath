// Handles updates to an existing task.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Dtos;

namespace SkillPath.Application.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskHandler
{
    private readonly ILearningTaskRepository _taskRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskHandler(ILearningTaskRepository taskRepository, ISkillRepository skillRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _skillRepository = skillRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LearningTaskDto?> HandleAsync(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var skill = await _skillRepository.GetByIdAsync(command.SkillId, cancellationToken);

        if (skill is null || skill.GoalId != command.GoalId)
            return null;

        var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

        if (task is null || task.SkillId != command.SkillId)
            return null;

        task.UpdateDetails(command.Title, command.Description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LearningTaskDto.FromEntity(task);
    }
}