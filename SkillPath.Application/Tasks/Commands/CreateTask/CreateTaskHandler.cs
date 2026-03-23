// Handles the creation of a new task within a skill.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Dtos;
using SkillPath.Domain.Entities;

namespace SkillPath.Application.Tasks.Commands.CreateTask;

public sealed class CreateTaskHandler
{
    private readonly ISkillRepository _skillRepository;
    private readonly ILearningTaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskHandler(ISkillRepository skillRepository, ILearningTaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        _skillRepository = skillRepository;
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LearningTaskDto?> HandleAsync(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var skill = await _skillRepository.GetByIdAsync(command.SkillId, cancellationToken);

        if (skill is null || skill.GoalId != command.GoalId)
            return null;

        var task = new LearningTask(command.SkillId, command.Title, command.Description, command.Order);

        await _taskRepository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LearningTaskDto.FromEntity(task);
    }
}