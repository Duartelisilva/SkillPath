// Exposes HTTP endpoints to manage tasks within a skill.
using Microsoft.AspNetCore.Mvc;
using SkillPath.API.Contracts.Tasks;
using SkillPath.Application.Tasks.Commands.CreateTask;
using SkillPath.Application.Tasks.Commands.DeleteTask;
using SkillPath.Application.Tasks.Commands.UpdateTask;
using SkillPath.Application.Tasks.Queries.GetTaskById;
using SkillPath.Application.Tasks.Queries.ListTasksBySkill;

namespace SkillPath.API.Controllers;

[ApiController]
[Route("api/goals/{goalId:guid}/skills/{skillId:guid}/tasks")]
public sealed class TasksController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        Guid goalId,
        Guid skillId,
        [FromServices] ListTasksBySkillHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new ListTasksBySkillQuery { GoalId = goalId, SkillId = skillId }, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetById(
        Guid goalId,
        Guid skillId,
        Guid taskId,
        [FromServices] GetTaskByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetTaskByIdQuery { GoalId = goalId, SkillId = skillId, TaskId = taskId }, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid goalId,
        Guid skillId,
        [FromBody] CreateTaskRequest request,
        [FromServices] CreateTaskHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand
        {
            GoalId = goalId,
            SkillId = skillId,
            Title = request.Title,
            Description = request.Description,
            Order = request.Order
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        if (result is null)
            return NotFound();

        return CreatedAtAction(nameof(GetById), new { goalId, skillId, taskId = result.Id }, result);
    }

    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> Update(
        Guid goalId,
        Guid skillId,
        Guid taskId,
        [FromBody] UpdateTaskRequest request,
        [FromServices] UpdateTaskHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand
        {
            GoalId = goalId,
            SkillId = skillId,
            TaskId = taskId,
            Title = request.Title,
            Description = request.Description
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(
        Guid goalId,
        Guid skillId,
        Guid taskId,
        [FromServices] DeleteTaskHandler handler,
        CancellationToken cancellationToken)
    {
        var deleted = await handler.HandleAsync(new DeleteTaskCommand { GoalId = goalId, SkillId = skillId, TaskId = taskId }, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}