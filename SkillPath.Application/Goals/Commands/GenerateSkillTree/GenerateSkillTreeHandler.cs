// Handles AI-powered skill tree generation for a goal.
using SkillPath.Application.Abstractions.AI;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Application.Skills.Dtos;
using SkillPath.Domain.Entities;
using SkillPath.Domain.Exceptions;

namespace SkillPath.Application.Goals.Commands.GenerateSkillTree;

public sealed class GenerateSkillTreeHandler
{
    private readonly IGoalRepository _goalRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly ILearningTaskRepository _taskRepository;
    private readonly ISkillTreeGenerator _skillGenerator;
    private readonly ITaskGenerator _taskGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateSkillTreeHandler(
        IGoalRepository goalRepository,
        ISkillRepository skillRepository,
        ILearningTaskRepository taskRepository,
        ISkillTreeGenerator skillGenerator,
        ITaskGenerator taskGenerator,
        IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _skillRepository = skillRepository;
        _taskRepository = taskRepository;
        _skillGenerator = skillGenerator;
        _taskGenerator = taskGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<SkillDto>> HandleAsync(
        GenerateSkillTreeCommand command,
        CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.GoalId, cancellationToken);

        if (goal is null)
            throw new DomainException("Goal not found.");

        // Delete existing skills (tasks cascade via DB)
        var existingSkills = await _skillRepository.ListByGoalAsync(goal.Id, cancellationToken);
        foreach (var existing in existingSkills)
            await _skillRepository.DeleteAsync(existing, cancellationToken);

        // Build context
        var goalTitle = command.AdditionalContext is null
            ? goal.Title
            : $"{goal.Title}. Additional context: {command.AdditionalContext}";

        // Generate skills
        var generatedSkills = await _skillGenerator.GenerateAsync(
            goalTitle,
            goal.Description,
            existingSkills.Select(s => s.Name).ToArray(),
            cancellationToken);

        // Persist skills and generate tasks for each
        var newSkills = new List<Skill>();
        foreach (var gen in generatedSkills)
        {
            var skill = new Skill(goal.Id, gen.Name, gen.Description, gen.Order);
            await _skillRepository.AddAsync(skill, cancellationToken);

            // Generate tasks for this skill
            var generatedTasks = await _taskGenerator.GenerateAsync(
                skill.Name,
                skill.Description,
                goal.Title,
                cancellationToken);

            foreach (var genTask in generatedTasks)
            {
                var task = new LearningTask(skill.Id, genTask.Title, genTask.Description, genTask.Order);
                await _taskRepository.AddAsync(task, cancellationToken);
            }

            newSkills.Add(skill);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newSkills.Select(SkillDto.FromEntity).ToArray();
    }
}