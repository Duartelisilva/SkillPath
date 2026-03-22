// Handles deletion of a task from a skill.
using SkillPath.Application.Abstractions.Persistence;

namespace SkillPath.Application.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskHandler(IGoalRepository goalRepository, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.GoalId, cancellationToken);

        if (goal is null)
            return false;

        var skill = goal.Skills.FirstOrDefault(s => s.Id == command.SkillId);

        if (skill is null)
            return false;

        skill.RemoveTask(command.TaskId);

        await _goalRepository.UpdateAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}