// Handles retrieval of a single task by identifier.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Dtos;

namespace SkillPath.Application.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdHandler
{
    private readonly ILearningTaskRepository _taskRepository;
    private readonly ISkillRepository _skillRepository;

    public GetTaskByIdHandler(ILearningTaskRepository taskRepository, ISkillRepository skillRepository)
    {
        _taskRepository = taskRepository;
        _skillRepository = skillRepository;
    }

    public async Task<LearningTaskDto?> HandleAsync(GetTaskByIdQuery query, CancellationToken cancellationToken)
    {
        var skill = await _skillRepository.GetByIdAsync(query.SkillId, cancellationToken);

        if (skill is null || skill.GoalId != query.GoalId)
            return null;

        var task = await _taskRepository.GetByIdAsync(query.TaskId, cancellationToken);

        if (task is null || task.SkillId != query.SkillId)
            return null;

        return LearningTaskDto.FromEntity(task);
    }
}