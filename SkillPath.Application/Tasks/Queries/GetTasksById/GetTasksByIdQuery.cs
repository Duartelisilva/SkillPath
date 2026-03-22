// Represents a request to retrieve a single task by identifier.
namespace SkillPath.Application.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdQuery
{
    public Guid GoalId { get; init; }
    public Guid SkillId { get; init; }
    public Guid TaskId { get; init; }
}