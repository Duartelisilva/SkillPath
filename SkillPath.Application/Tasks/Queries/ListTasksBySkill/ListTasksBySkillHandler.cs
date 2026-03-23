// Handles retrieval of all tasks for a given skill.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Dtos;

namespace SkillPath.Application.Tasks.Queries.ListTasksBySkill;

public sealed class ListTasksBySkillHandler
{
    private readonly ISkillRepository _skillRepository;
    private readonly ILearningTaskRepository _taskRepository;

    public ListTasksBySkillHandler(ISkillRepository skillRepository, ILearningTaskRepository taskRepository)
    {
        _skillRepository = skillRepository;
        _taskRepository = taskRepository;
    }

    public async Task<IReadOnlyCollection<LearningTaskDto>?> HandleAsync(ListTasksBySkillQuery query, CancellationToken cancellationToken)
    {
        var skill = await _skillRepository.GetByIdAsync(query.SkillId, cancellationToken);

        if (skill is null || skill.GoalId != query.GoalId)
            return null;

        var tasks = await _taskRepository.ListBySkillAsync(query.SkillId, cancellationToken);

        return tasks
            .Select(LearningTaskDto.FromEntity)
            .ToArray();
    }
}