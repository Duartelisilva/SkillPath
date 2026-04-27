// Exposes HTTP endpoints to manage learning goals.
using Microsoft.AspNetCore.Mvc;
using SkillPath.API.Contracts.Goals;
using SkillPath.Application.Goals.Commands.CreateGoal;
using SkillPath.Application.Goals.Commands.DeleteGoal;
using SkillPath.Application.Goals.Commands.GenerateSkillTree;
using SkillPath.Application.Goals.Commands.UpdateGoal;
using SkillPath.Application.Goals.Queries.GetGoalById;
using SkillPath.Application.Goals.Queries.ListGoals;

namespace SkillPath.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GoalsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromServices] ListGoalsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new ListGoalsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] GetGoalByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetGoalByIdQuery { Id = id }, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateGoalRequest request,
        [FromServices] CreateGoalHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateGoalCommand
        {
            Title = request.Title,
            Description = request.Description
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateGoalRequest request,
        [FromServices] UpdateGoalHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateGoalCommand
        {
            Id = id,
            Title = request.Title,
            Description = request.Description
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteGoalHandler handler,
        CancellationToken cancellationToken)
    {
        var deleted = await handler.HandleAsync(new DeleteGoalCommand { Id = id }, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/generate-skill-tree")]
    public async Task<IActionResult> GenerateSkillTree(
    Guid id,
    [FromBody] GenerateSkillTreeRequest request,
    [FromServices] GenerateSkillTreeHandler handler,
    CancellationToken cancellationToken)
    {
        var command = new GenerateSkillTreeCommand
        {
            GoalId = id,
            AdditionalContext = request.AdditionalContext,
            MinSkills = request.MinSkills ?? 5,
            MaxSkills = request.MaxSkills ?? 12,
            TasksPerSkill = request.TasksPerSkill ?? 5,
            Difficulty = Enum.TryParse<DifficultyLevel>(request.Difficulty, out var diff)
                ? diff
                : DifficultyLevel.Intermediate,
            Focus = Enum.TryParse<TreeFocus>(request.Focus, out var focus)
                ? focus
                : TreeFocus.Balanced
        };

        var result = await handler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }
}