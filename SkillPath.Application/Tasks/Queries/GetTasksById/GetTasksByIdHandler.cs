// Handles retrieval of a single task by identifier.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Dtos;

namespace SkillPath.Application.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdHandler
{
    private readonly IGoalRepository _goalRepository;

    public GetTaskByIdHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<LearningTaskDto?> HandleAsync(GetTaskByIdQuery query, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(query.GoalId, cancellationToken);

        if (goal is null)
            return null;

        var skill = goal.Skills.FirstOrDefault(s => s.Id == query.SkillId);

        if (skill is null)
            return null;

        var task = skill.Tasks.FirstOrDefault(t => t.Id == query.TaskId);

        return task is null ? null : LearningTaskDto.FromEntity(task);
    }
}