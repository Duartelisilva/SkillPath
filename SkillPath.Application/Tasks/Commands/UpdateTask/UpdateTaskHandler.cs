// Handles updates to an existing task.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Dtos;

namespace SkillPath.Application.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskHandler(IGoalRepository goalRepository, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LearningTaskDto?> HandleAsync(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.GoalId, cancellationToken);

        if (goal is null)
            return null;

        var skill = goal.Skills.FirstOrDefault(s => s.Id == command.SkillId);

        if (skill is null)
            return null;

        var task = skill.Tasks.FirstOrDefault(t => t.Id == command.TaskId);

        if (task is null)
            return null;

        task.UpdateDetails(command.Title, command.Description);

        await _goalRepository.UpdateAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LearningTaskDto.FromEntity(task);
    }
}