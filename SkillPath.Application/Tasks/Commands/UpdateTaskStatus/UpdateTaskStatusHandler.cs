// Handles updating a task's status and auto-completes skills/goals when appropriate.
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Dtos;
using SkillPath.Domain.Enums;

namespace SkillPath.Application.Tasks.Commands.UpdateTaskStatus;

public sealed class UpdateTaskStatusHandler
{
    private readonly ILearningTaskRepository _taskRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskStatusHandler(
        ILearningTaskRepository taskRepository,
        ISkillRepository skillRepository,
        IGoalRepository goalRepository,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _skillRepository = skillRepository;
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LearningTaskDto?> HandleAsync(
        UpdateTaskStatusCommand command,
        CancellationToken cancellationToken)
    {
        // Validate ownership chain
        var skill = await _skillRepository.GetByIdAsync(command.SkillId, cancellationToken);
        if (skill is null || skill.GoalId != command.GoalId)
            return null;

        var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);
        if (task is null || task.SkillId != command.SkillId)
            return null;

        // Update task status
        var newStatus = command.Status switch
        {
            "NotStarted" => LearningTaskStatus.NotStarted,
            "InProgress" => LearningTaskStatus.InProgress,
            "Completed" => LearningTaskStatus.Completed,
            _ => task.Status
        };

        switch (newStatus)
        {
            case LearningTaskStatus.NotStarted:
                task.Reset();
                break;
            case LearningTaskStatus.InProgress:
                task.Start();
                break;
            case LearningTaskStatus.Completed:
                task.Complete();
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Check if all tasks in this skill are completed
        await CheckAndCompleteSkillAsync(skill, cancellationToken);

        // Check if all skills in the goal are completed
        await CheckAndCompleteGoalAsync(command.GoalId, cancellationToken);

        return LearningTaskDto.FromEntity(task);
    }

    private async Task CheckAndCompleteSkillAsync(
        Domain.Entities.Skill skill,
        CancellationToken cancellationToken)
    {
        var allTasks = await _taskRepository.ListBySkillAsync(skill.Id, cancellationToken);

        if (allTasks.Count == 0)
            return;

        var allCompleted = allTasks.All(t => t.Status == LearningTaskStatus.Completed);

        if (allCompleted && skill.Status != SkillStatus.Completed)
        {
            skill.Complete();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Unlock dependent skills
            await UnlockDependentSkillsAsync(skill.GoalId, skill.Id, cancellationToken);
        }
    }

    private async Task UnlockDependentSkillsAsync(
        Guid goalId,
        Guid completedSkillId,
        CancellationToken cancellationToken)
    {
        var allSkills = await _skillRepository.ListByGoalAsync(goalId, cancellationToken);

        foreach (var skill in allSkills)
        {
            if (skill.Status == SkillStatus.Locked && skill.DependsOn.Contains(completedSkillId))
            {
                // Check if all dependencies are completed
                var allDepsCompleted = true;
                foreach (var depId in skill.DependsOn)
                {
                    var depSkill = allSkills.FirstOrDefault(s => s.Id == depId);
                    if (depSkill?.Status != SkillStatus.Completed)
                    {
                        allDepsCompleted = false;
                        break;
                    }
                }

                if (allDepsCompleted)
                {
                    skill.Unlock();
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckAndCompleteGoalAsync(
        Guid goalId,
        CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId, cancellationToken);
        if (goal is null)
            return;

        var allSkills = await _skillRepository.ListByGoalAsync(goalId, cancellationToken);

        if (allSkills.Count == 0)
            return;

        var allCompleted = allSkills.All(s => s.Status == SkillStatus.Completed);

        if (allCompleted && goal.Status != GoalStatus.Completed)
        {
            goal.Complete();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}