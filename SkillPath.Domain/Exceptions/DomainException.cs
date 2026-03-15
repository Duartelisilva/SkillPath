// Represents business rule violations inside the domain layer.
namespace SkillPath.Domain.Exceptions;

public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
