// Handles AI-powered skill tree generation for a goal with dependency mapping.
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

        // First pass: Create all skills
        var skillMap = new Dictionary<int, Skill>(); // order -> skill
        var newSkills = new List<Skill>();

        foreach (var gen in generatedSkills)
        {
            var skill = new Skill(goal.Id, gen.Name, gen.Description, gen.Order);
            await _skillRepository.AddAsync(skill, cancellationToken);
            skillMap[gen.Order] = skill;
            newSkills.Add(skill);
        }

        // Save to get IDs assigned
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Second pass: Map dependencies using order numbers
        // Parse the dependencies from the AI response by re-calling with a modified interface
        // For now, we'll create a simple sequential dependency chain
        // TODO: Enhance to parse actual dependency data from AI
        for (int i = 1; i < newSkills.Count; i++)
        {
            var currentSkill = newSkills[i];
            var previousSkill = newSkills[i - 1];
            currentSkill.AddDependency(previousSkill.Id);
        }

        // Third pass: Generate tasks for each skill
        foreach (var skill in newSkills)
        {
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
        }

        // Unlock the first skill (no dependencies)
        if (newSkills.Count > 0)
        {
            newSkills[0].Unlock();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newSkills.Select(SkillDto.FromEntity).ToArray();
    }
}