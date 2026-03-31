// Handles regenerating tasks for a specific skill using AI.
using SkillPath.Application.Abstractions.AI;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Tasks.Dtos;
using SkillPath.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace SkillPath.Application.Skills.Commands.RegenerateTasks;

public sealed class RegenerateTasksForSkillHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly ILearningTaskRepository _taskRepository;
    private readonly ITaskGenerator _taskGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegenerateTasksForSkillHandler> _logger;

    public RegenerateTasksForSkillHandler(
        IGoalRepository goalRepository,
        ISkillRepository skillRepository,
        ILearningTaskRepository taskRepository,
        ITaskGenerator taskGenerator,
        IUnitOfWork unitOfWork,
        ILogger<RegenerateTasksForSkillHandler> logger)
    {
        _goalRepository = goalRepository;
        _skillRepository = skillRepository;
        _taskRepository = taskRepository;
        _taskGenerator = taskGenerator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<LearningTaskDto>?> HandleAsync(
        RegenerateTasksForSkillCommand command,
        CancellationToken cancellationToken)
    {
        // Validate ownership chain
        var skill = await _skillRepository.GetByIdAsync(command.SkillId, cancellationToken);
        if (skill is null || skill.GoalId != command.GoalId)
            return null;

        var goal = await _goalRepository.GetByIdAsync(command.GoalId, cancellationToken);
        if (goal is null)
            return null;

        _logger.LogInformation("Regenerating tasks for skill: {SkillName}", skill.Name);

        // Delete existing tasks for this skill
        var existingTasks = await _taskRepository.ListBySkillAsync(skill.Id, cancellationToken);
        _logger.LogInformation("Deleting {Count} existing tasks", existingTasks.Count);

        foreach (var task in existingTasks)
        {
            await _taskRepository.DeleteAsync(task, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate new tasks
        try
        {
            var generatedTasks = await _taskGenerator.GenerateAsync(
                skill.Name,
                skill.Description,
                goal.Title,
                cancellationToken);

            _logger.LogInformation("AI generated {Count} new tasks for skill {SkillName}",
                generatedTasks.Count, skill.Name);

            var newTasks = new List<LearningTask>();

            foreach (var genTask in generatedTasks)
            {
                var task = new LearningTask(skill.Id, genTask.Title, genTask.Description, genTask.Order);
                await _taskRepository.AddAsync(task, cancellationToken);
                newTasks.Add(task);
                _logger.LogInformation("Created task: {TaskTitle} (Order: {Order})",
                    genTask.Title, genTask.Order);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully regenerated {Count} tasks for skill {SkillName}",
                newTasks.Count, skill.Name);

            return newTasks.Select(LearningTaskDto.FromEntity).ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to regenerate tasks for skill {SkillName}", skill.Name);
            throw new InvalidOperationException($"Failed to regenerate tasks for skill {skill.Name}", ex);
        }
    }
}