// Exposes HTTP endpoints to manage skills within a learning goal.
using Microsoft.AspNetCore.Mvc;
using SkillPath.API.Contracts.Skills;
using SkillPath.Application.Skills.Commands.CreateSkill;
using SkillPath.Application.Skills.Commands.DeleteSkill;
using SkillPath.Application.Skills.Commands.UpdateSkill;
using SkillPath.Application.Skills.Queries.GetSkillById;
using SkillPath.Application.Skills.Queries.ListSkillsByGoal;

namespace SkillPath.API.Controllers;

[ApiController]
[Route("api/goals/{goalId:guid}/skills")]
public sealed class SkillsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        Guid goalId,
        [FromServices] ListSkillsByGoalHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new ListSkillsByGoalQuery { GoalId = goalId }, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{skillId:guid}")]
    public async Task<IActionResult> GetById(
        Guid goalId,
        Guid skillId,
        [FromServices] GetSkillByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetSkillByIdQuery { GoalId = goalId, SkillId = skillId }, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid goalId,
        [FromBody] CreateSkillRequest request,
        [FromServices] CreateSkillHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateSkillCommand
        {
            GoalId = goalId,
            Name = request.Name,
            Description = request.Description,
            Order = request.Order
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        if (result is null)
            return NotFound();

        return CreatedAtAction(nameof(GetById), new { goalId, skillId = result.Id }, result);
    }

    [HttpPut("{skillId:guid}")]
    public async Task<IActionResult> Update(
        Guid goalId,
        Guid skillId,
        [FromBody] UpdateSkillRequest request,
        [FromServices] UpdateSkillHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSkillCommand
        {
            GoalId = goalId,
            SkillId = skillId,
            Name = request.Name,
            Description = request.Description
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{skillId:guid}")]
    public async Task<IActionResult> Delete(
        Guid goalId,
        Guid skillId,
        [FromServices] DeleteSkillHandler handler,
        CancellationToken cancellationToken)
    {
        var deleted = await handler.HandleAsync(new DeleteSkillCommand { GoalId = goalId, SkillId = skillId }, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}