// Handles the creation of a new task within a skill.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Dtos;

namespace SkillPath.Application.Tasks.Commands.CreateTask;

public sealed class CreateTaskHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskHandler(IGoalRepository goalRepository, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LearningTaskDto?> HandleAsync(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.GoalId, cancellationToken);

        if (goal is null)
            return null;

        var skill = goal.Skills.FirstOrDefault(s => s.Id == command.SkillId);

        if (skill is null)
            return null;

        var task = skill.AddTask(command.Title, command.Description, command.Order);

        await _goalRepository.UpdateAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LearningTaskDto.FromEntity(task);
    }
}