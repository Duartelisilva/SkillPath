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
    private readonly ISkillTreeGenerator _generator;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateSkillTreeHandler(
        IGoalRepository goalRepository,
        ISkillRepository skillRepository,
        ISkillTreeGenerator generator,
        IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _skillRepository = skillRepository;
        _generator = generator;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<SkillDto>> HandleAsync(
        GenerateSkillTreeCommand command,
        CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(command.GoalId, cancellationToken);

        if (goal is null)
            throw new DomainException("Goal not found.");

        // Delete existing skills
        var existingSkills = await _skillRepository.ListByGoalAsync(goal.Id, cancellationToken);
        foreach (var existing in existingSkills)
            await _skillRepository.DeleteAsync(existing, cancellationToken);

        // Build context from goal + any additional context
        var goalTitle = command.AdditionalContext is null
            ? goal.Title
            : $"{goal.Title}. Additional context: {command.AdditionalContext}";

        var generated = await _generator.GenerateAsync(
            goalTitle,
            goal.Description,
            existingSkills.Select(s => s.Name).ToArray(),
            cancellationToken);

        // Persist new skills
        var newSkills = new List<Skill>();
        foreach (var gen in generated)
        {
            var skill = new Skill(goal.Id, gen.Name, gen.Description, gen.Order);
            await _skillRepository.AddAsync(skill, cancellationToken);
            newSkills.Add(skill);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newSkills.Select(SkillDto.FromEntity).ToArray();
    }
}