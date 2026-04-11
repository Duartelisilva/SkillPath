// Handles AI-powered skill tree generation for a goal with dependency mapping.
using SkillPath.Application.Abstractions.AI;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Dtos;
using SkillPath.Domain.Entities;
using SkillPath.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace SkillPath.Application.Goals.Commands.GenerateSkillTree;

public sealed class GenerateSkillTreeHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly ILearningTaskRepository _taskRepository;
    private readonly ISkillTreeGenerator _skillGenerator;
    private readonly ITaskGenerator _taskGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenerateSkillTreeHandler> _logger;

    public GenerateSkillTreeHandler(
        IGoalRepository goalRepository,
        ISkillRepository skillRepository,
        ILearningTaskRepository taskRepository,
        ISkillTreeGenerator skillGenerator,
        ITaskGenerator taskGenerator,
        IUnitOfWork unitOfWork,
        ILogger<GenerateSkillTreeHandler> logger)
    {
        _goalRepository = goalRepository;
        _skillRepository = skillRepository;
        _taskRepository = taskRepository;
        _skillGenerator = skillGenerator;
        _taskGenerator = taskGenerator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<SkillDto>> HandleAsync(
        GenerateSkillTreeCommand command,
        CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.GoalId, cancellationToken);

        if (goal is null)
            throw new DomainException("Goal not found.");

        _logger.LogInformation("Starting skill tree generation for goal {GoalId}: {GoalTitle}", goal.Id, goal.Title);

        // Delete existing skills (tasks cascade via DB)
        var existingSkills = await _skillRepository.ListByGoalAsync(goal.Id, cancellationToken);
        _logger.LogInformation("Deleting {Count} existing skills", existingSkills.Count);

        foreach (var existing in existingSkills)
            await _skillRepository.DeleteAsync(existing, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Build context
        var goalTitle = command.AdditionalContext is null
            ? goal.Title
            : $"{goal.Title}. Additional context: {command.AdditionalContext}";

        // Generate skills
        _logger.LogInformation("Requesting AI to generate skills for: {GoalTitle}", goalTitle);
        var generatedSkills = await _skillGenerator.GenerateAsync(
            goalTitle,
            goal.Description,
            Array.Empty<string>(),
            cancellationToken);

        _logger.LogInformation("AI generated {Count} skills", generatedSkills.Count);

        // First pass: Create all skills
        var skillMap = new Dictionary<int, Skill>(); // order -> skill
        var newSkills = new List<Skill>();

        foreach (var gen in generatedSkills)
        {
            _logger.LogInformation("Creating skill: {SkillName} (Order: {Order})", gen.Name, gen.Order);
            var skill = new Skill(goal.Id, gen.Name, gen.Description, gen.Order);
            await _skillRepository.AddAsync(skill, cancellationToken);
            skillMap[gen.Order] = skill;
            newSkills.Add(skill);
        }

        // Save to get IDs assigned
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Skills created and saved to database");

        // Second pass: Map dependencies using sequential chain
        _logger.LogInformation("Setting up skill dependencies");
        for (int i = 1; i < newSkills.Count; i++)
        {
            var currentSkill = newSkills[i];
            var previousSkill = newSkills[i - 1];
            currentSkill.AddDependency(previousSkill.Id);
            _logger.LogInformation("Skill '{CurrentSkill}' depends on '{PreviousSkill}'",
                currentSkill.Name, previousSkill.Name);
        }

        // Third pass: Generate tasks for each skill
        _logger.LogInformation("Starting task generation for {Count} skills", newSkills.Count);
        var totalTasksGenerated = 0;

        foreach (var skill in newSkills)
        {
            _logger.LogInformation("Generating tasks for skill: {SkillName}", skill.Name);

            try
            {
                var generatedTasks = await _taskGenerator.GenerateAsync(
                    skill.Name,
                    skill.Description,
                    goal.Title,
                    skill.RequiredExperiencePoints,
                    cancellationToken);

                _logger.LogInformation("AI generated {Count} tasks for skill {SkillName}",
                    generatedTasks.Count, skill.Name);

                foreach (var genTask in generatedTasks)
                {
                    var task = new LearningTask(skill.Id, genTask.Title, genTask.Description, genTask.Order, genTask.ExperiencePoints);
                    await _taskRepository.AddAsync(task, cancellationToken);
                    totalTasksGenerated++;
                    _logger.LogInformation("Created task: {TaskTitle} (Order: {Order}, XP: {XP})",
                        genTask.Title, genTask.Order, genTask.ExperiencePoints);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate tasks for skill {SkillName}. Continuing with next skill.",
                    skill.Name);
                // Continue with other skills even if one fails
            }
        }

        _logger.LogInformation("Total tasks generated: {TotalTasks}", totalTasksGenerated);

        // Unlock the first skill (no dependencies)
        if (newSkills.Count > 0)
        {
            newSkills[0].Unlock();
            _logger.LogInformation("Unlocked first skill: {SkillName}", newSkills[0].Name);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Skill tree generation completed successfully");

        return newSkills.Select(SkillDto.FromEntity).ToArray();
    }
}